using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Void.Bot.IntegrationTest.Services.ERA;

/// <summary>仅控制指定测试 SQL 的时序与失败点，业务层、DataProvider 和数据库均使用真实实现。</summary>
internal sealed class EraIntegrationSqlInterceptor : DbCommandInterceptor
{
    private int _startedUpdates;
    private int _completedUpdates;
    private int _affectedRows;

    public bool PauseFirstUpdate { get; init; }
    public int ForceZeroRowsOnUpdate { get; init; }
    public bool FailReceiptInsert { get; init; }
    public bool FailWeaponConfigurationInsert { get; init; }
    public bool PauseWorldSave { get; init; }
    public bool FailWorldSave { get; init; }
    public bool FailRewardLinkInsert { get; init; }
    public bool PauseRewardSave { get; init; }
    public bool PauseRewardGrantSave { get; init; }
    public bool PauseDropSave { get; init; }
    public bool RewardFailureInjected { get; private set; }
    public bool WorldFailureInjected { get; private set; }
    public bool ReceiptFailureInjected { get; private set; }
    public int CompletedUpdates
    {
        get
        {
            return Volatile.Read(ref _completedUpdates);
        }
    }

    public int AffectedRows
    {
        get
        {
            return Volatile.Read(ref _affectedRows);
        }
    }

    public TaskCompletionSource UpdateReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource ResumeUpdate { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource PlayerLockReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource WorldLockReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource WorldSaveReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource ResumeWorldSave { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource TeamLockReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource WorldRestrictionReadReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource RewardSaveReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource ResumeRewardSave { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource RewardScopeReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource DropSaveReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource ResumeDropSave { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (command.CommandText.Contains("INSERT INTO era_world_team_state", StringComparison.OrdinalIgnoreCase)) TeamLockReached.TrySetResult();
        if (command.CommandText.Contains("INSERT INTO era_reward_scope", StringComparison.OrdinalIgnoreCase)) RewardScopeReached.TrySetResult();
        if (command.CommandText.Contains("UPDATE ", StringComparison.OrdinalIgnoreCase) &&
            command.CommandText.Contains("era_equipment_instance", StringComparison.OrdinalIgnoreCase))
        {
            var number = Interlocked.Increment(ref _startedUpdates);
            if (PauseFirstUpdate && number == 1)
            {
                UpdateReached.TrySetResult();
                await ResumeUpdate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            if (number == ForceZeroRowsOnUpdate)
            {
                // 保留真实 UPDATE，让 MySQL 返回 0 行，进入生产代码的条件更新冲突路径。
                command.CommandText = command.CommandText.TrimEnd().TrimEnd(';') + " AND 1 = 0";
            }
        }

        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        if (command.CommandText.Contains("UPDATE ", StringComparison.OrdinalIgnoreCase) &&
            command.CommandText.Contains("era_equipment_instance", StringComparison.OrdinalIgnoreCase))
        {
            Interlocked.Increment(ref _completedUpdates);
            Interlocked.Add(ref _affectedRows, result);
        }

        return ValueTask.FromResult(result);
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        if (FailWeaponConfigurationInsert && command.CommandText.Contains("INSERT INTO `era_weapon_template_mod_slot`", StringComparison.OrdinalIgnoreCase))
        {
            var failureAt = command.CommandText.IndexOf("INSERT INTO `era_weapon_template_mod_slot`", StringComparison.OrdinalIgnoreCase);
            command.CommandText = command.CommandText[..failureAt] + "SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'ERA weapon configuration integration failure';";
        }
        if (PauseDropSave && (command.CommandText.Contains("UPDATE `era_equipment_drop`", StringComparison.OrdinalIgnoreCase) ||
            command.CommandText.Contains("INSERT INTO `era_equipment_drop`", StringComparison.OrdinalIgnoreCase)))
        {
            DropSaveReached.TrySetResult();
            await ResumeDropSave.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        if (PauseRewardSave && command.CommandText.Contains("INSERT INTO `era_reward_event`", StringComparison.OrdinalIgnoreCase))
        {
            RewardSaveReached.TrySetResult();
            await ResumeRewardSave.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        if (PauseRewardGrantSave && command.CommandText.Contains("INSERT INTO `era_reward_grant`", StringComparison.OrdinalIgnoreCase))
        {
            RewardSaveReached.TrySetResult();
            await ResumeRewardSave.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        if (FailRewardLinkInsert && command.CommandText.Contains("INSERT INTO `era_reward_grant_equipment`", StringComparison.OrdinalIgnoreCase))
        {
            RewardFailureInjected = true;
            var failureAt = command.CommandText.IndexOf("INSERT INTO `era_reward_grant_equipment`", StringComparison.OrdinalIgnoreCase);
            command.CommandText = command.CommandText[..failureAt] + "SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'ERA reward integration failure';";
        }
        if (command.CommandText.Contains("FROM era_world AS world", StringComparison.OrdinalIgnoreCase) &&
            command.CommandText.Contains("FOR SHARE", StringComparison.OrdinalIgnoreCase)) WorldRestrictionReadReached.TrySetResult();
        if (command.CommandText.Contains("FROM era_world WHERE", StringComparison.OrdinalIgnoreCase) &&
            command.CommandText.Contains("FOR UPDATE", StringComparison.OrdinalIgnoreCase)) WorldLockReached.TrySetResult();
        if (PauseWorldSave && command.CommandText.Contains("UPDATE `era_world`", StringComparison.OrdinalIgnoreCase))
        {
            WorldSaveReached.TrySetResult();
            await ResumeWorldSave.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        if (FailWorldSave && command.CommandText.Contains("UPDATE `era_world`", StringComparison.OrdinalIgnoreCase))
        {
            WorldFailureInjected = true;
            var failureAt = command.CommandText.IndexOf("UPDATE `era_world`", StringComparison.OrdinalIgnoreCase);
            command.CommandText = command.CommandText[..failureAt] + "SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'ERA world lifetime integration failure';";
        }
        if (command.CommandText.Contains("FROM era_player ", StringComparison.OrdinalIgnoreCase) &&
            command.CommandText.Contains("FOR UPDATE", StringComparison.OrdinalIgnoreCase))
        {
            PlayerLockReached.TrySetResult();
        }

        if (FailReceiptInsert && command.CommandText.Contains("INSERT INTO", StringComparison.OrdinalIgnoreCase) &&
            command.CommandText.Contains("era_equipment_operation_receipt", StringComparison.OrdinalIgnoreCase))
        {
            // 保留同批次中失败点之前的真实写入与参数，让 MySQL 在回执处失败，再核对整笔回滚。
            ReceiptFailureInjected = true;
            var failureAt = command.CommandText.IndexOf("INSERT INTO `era_equipment_operation_receipt`", StringComparison.OrdinalIgnoreCase);
            if (failureAt < 0)
            {
                throw new InvalidOperationException("Equipment receipt failure point was not found.");
            }
            command.CommandText = command.CommandText[..failureAt] + "SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'ERA equipment integration failure';";
        }

        return result;
    }
}
