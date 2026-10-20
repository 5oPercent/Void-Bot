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
    public bool PauseWorldSave { get; init; }
    public bool FailWorldSave { get; init; }
    public bool WorldFailureInjected { get; private set; }
    public bool ReceiptFailureInjected { get; private set; }
    public int CompletedUpdates => Volatile.Read(ref _completedUpdates);
    public int AffectedRows => Volatile.Read(ref _affectedRows);
    public TaskCompletionSource UpdateReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource ResumeUpdate { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource PlayerLockReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource WorldLockReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource WorldSaveReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource ResumeWorldSave { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource TeamLockReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource WorldRestrictionReadReached { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (command.CommandText.Contains("INSERT INTO era_world_team_state", StringComparison.OrdinalIgnoreCase)) TeamLockReached.TrySetResult();
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
            command.Parameters.Clear();
            command.CommandText = "SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'ERA world lifetime integration failure'";
        }
        if (command.CommandText.Contains("FROM era_player ", StringComparison.OrdinalIgnoreCase) &&
            command.CommandText.Contains("FOR UPDATE", StringComparison.OrdinalIgnoreCase))
        {
            PlayerLockReached.TrySetResult();
        }

        if (FailReceiptInsert && command.CommandText.Contains("INSERT INTO", StringComparison.OrdinalIgnoreCase) &&
            command.CommandText.Contains("era_equipment_operation_receipt", StringComparison.OrdinalIgnoreCase))
        {
            // SaveChanges 的 MySQL 批处理通过 reader 执行；让真实服务器拒绝最后的保存批次。
            ReceiptFailureInjected = true;
            command.Parameters.Clear();
            command.CommandText = "SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'ERA equipment integration failure'";
        }

        return result;
    }
}
