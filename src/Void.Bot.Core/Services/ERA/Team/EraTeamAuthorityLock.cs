using System.Data;
using MySqlConnector;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.Core.Services.ERA.Team;

/// <summary>Redis 提交的 SQL 协调锁；不参与调用方资产事务，不在可重试 CAS 回调中执行。</summary>
internal sealed class EraTeamAuthorityLock : IAsyncDisposable
{
    private readonly MySqlConnection _connection;
    private readonly MySqlTransaction _transaction;

    private EraTeamAuthorityLock(MySqlConnection connection, MySqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public static async Task<EraTeamAuthorityLock> AcquireAsync(ConnectionStringSetting setting,
        IEnumerable<Guid> teamIds, CancellationToken cancellationToken)
    {
        var connection = new MySqlConnection(setting.Mysql);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken).ConfigureAwait(false);
            try
            {
                // 切队一次锁两队，所有调用者保持相同排序。
                foreach (var id in teamIds.Distinct().OrderBy(id => id.ToString("D"), StringComparer.Ordinal))
                {
                    await using var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.Parameters.AddWithValue("@teamId", id.ToString("D"));
                    command.CommandText = "INSERT INTO era_world_team_state (team_id, active_world_id) VALUES (@teamId, NULL) ON DUPLICATE KEY UPDATE team_id = team_id";
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    command.CommandText = "SELECT team_id FROM era_world_team_state WHERE team_id = @teamId FOR UPDATE";
                    await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                }
                return new EraTeamAuthorityLock(connection, transaction);
            }
            catch
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public Task CommitAsync()
    {
        // Redis 一旦发出不取消等待；此提交只持久化新锁行，不保存任何装备或世界状态。
        return _transaction.CommitAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        try { await _transaction.DisposeAsync().ConfigureAwait(false); }
        finally { await _connection.DisposeAsync().ConfigureAwait(false); }
    }
}
