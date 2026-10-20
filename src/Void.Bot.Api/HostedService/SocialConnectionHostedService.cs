using Microsoft.EntityFrameworkCore;
using Void.Bot.Api.Hubs;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Services.ERA.Team;

namespace Void.Bot.Api.HostedService;

/// <summary>定期批量核验长连接身份；通知连接从不续期游戏会话。</summary>
public sealed class SocialConnectionHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly SocialConnectionRegistry _connections;
    private readonly ILogger<SocialConnectionHostedService> _logger;

    public SocialConnectionHostedService(IServiceScopeFactory scopes,
        SocialConnectionRegistry connections, ILogger<SocialConnectionHostedService> logger)
    {
        _scopes = scopes;
        _connections = connections;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<VoidBotDbContext>();
                var sessions = scope.ServiceProvider.GetRequiredService<IEraTeamDataProvider>();
                var snapshot = _connections.Snapshot();
                foreach (var batch in snapshot.Chunk(128))
                {
                    var ids = batch.Select(value => value.AccountId).Distinct().ToArray();
                    var accounts = await database.Set<UserAccount>().AsNoTracking()
                        .Where(value => ids.Contains(value.Id))
                        .Select(value => new { value.Id, value.TokenVersion })
                        .ToDictionaryAsync(value => value.Id, value => value.TokenVersion, stoppingToken)
                        .ConfigureAwait(false);
                    foreach (var connection in batch)
                    {
                        var valid = connection.TokenExpiresAt > DateTimeOffset.UtcNow &&
                            accounts.TryGetValue(connection.AccountId, out var version) &&
                            version == connection.TokenVersion;
                        if (valid && connection.EraClientId.HasValue)
                        {
                            valid = await sessions.IsClientSessionActiveAsync(connection.AccountId,
                                connection.EraClientId.Value, stoppingToken).ConfigureAwait(false);
                        }
                        if (!valid)
                        {
                            connection.Abort();
                            _connections.Remove(connection.ConnectionId);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Social connection validation failed.");
                // 无法证明身份仍有效时关闭连接，客户端通过带抖动的重连重新认证。
                foreach (var connection in _connections.Snapshot())
                {
                    connection.Abort();
                    _connections.Remove(connection.ConnectionId);
                }
            }
        }
    }
}
