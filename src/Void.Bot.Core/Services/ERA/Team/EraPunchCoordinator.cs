using System.Net;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using Void.Bot.Core.Settings.ERA;

namespace Void.Bot.Core.Services.ERA.Team;

public interface IEraPunchCoordinator : ISingleton
{
    Task RemoveExpiredAsync(CancellationToken cancellationToken);
    Task RegisterAsync(Guid teamId, Guid memberId, Guid attemptId, string token, IPEndPoint endpoint, CancellationToken cancellationToken);
}

public class EraPunchCoordinator : IEraPunchCoordinator
{
    private readonly IEraTeamDataProvider _dataProvider;
    private readonly EraNetworkingSetting _setting;
    private readonly ILogger _logger;
    private readonly Stopwatch _rejectionWindow = Stopwatch.StartNew();

    public EraPunchCoordinator(IEraTeamDataProvider dataProvider, EraNetworkingSetting setting, Serilog.ILogger logger)
    {
        _dataProvider = dataProvider;
        _setting = setting;
        _logger = logger;
    }

    public async Task RemoveExpiredAsync(CancellationToken cancellationToken)
    {
        await _dataProvider.RemoveExpiredIndexAsync(cancellationToken).ConfigureAwait(false);
        await foreach (var snapshot in _dataProvider.ReadTeamsForCleanupAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                await _dataProvider.ChangeAsync(snapshot.TeamId, team =>
                {
                    if (team == null)
                    {
                        return new EraTeamChange<bool>
                        {
                            Result = false
                        };
                    }

                    var now = DateTimeOffset.UtcNow;
                    var previousVersion = System.Text.Json.JsonSerializer.Serialize(team);
                    var alive = team.ExpiresAt > now && EraTeamLifecycle.Advance(team, now, _setting.MigrationTimeoutSeconds);
                    // 端点/票据失效可能不修改成员字段，仍需提交对端有效性变化。
                    if (alive) EraPeersState.Refresh(team, now);
                    var changed = !alive || previousVersion != System.Text.Json.JsonSerializer.Serialize(team);
                    return new EraTeamChange<bool>
                    {
                        Changed = changed,
                        Team = alive ? team : null,
                        Result = changed
                    };
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (EraTeamWriteException)
            {
                // 与心跳、退出或重新加入并发时下一轮重新检查，不用扫描快照覆盖最新会话。
            }
        }
    }

    public async Task RegisterAsync(
        Guid teamId,
        Guid memberId,
        Guid attemptId,
        string token,
        IPEndPoint endpoint,
        CancellationToken cancellationToken)
    {
        if (!_setting.Enabled || token.Length != 44)
        {
            LogRejected(teamId, memberId, endpoint, "networking_disabled_or_invalid_token_length");
            return;
        }

        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var rejection = string.Empty;
        var endpointChanged = false;
        try
        {
            var registered = await _dataProvider.ChangeAsync(teamId, team =>
            {
                var now = DateTimeOffset.UtcNow;
                rejection = string.Empty;
                endpointChanged = false;
                if (team == null || team.ExpiresAt <= now ||
                    !team.Members.Any(item => item.ExpiresAt > now))
                {
                    rejection = "team_missing_or_expired";
                    return new EraTeamChange<bool>
                    {
                        Result = false
                    };
                }

                var member = team.Members.SingleOrDefault(item => item.MemberId == memberId);
                if (member == null)
                {
                    rejection = "member_not_found";
                }
                else if (member.ExpiresAt <= now)
                {
                    rejection = "member_expired";
                }
                else if (member.TicketAuthorityEpoch != team.AuthorityEpoch)
                {
                    rejection = "authority_epoch_changed";
                }
                else if (member.AttemptId != attemptId)
                {
                    rejection = "attempt_changed";
                }
                else if (member.TicketExpiresAt <= now)
                {
                    rejection = "ticket_expired";
                }
                else if (member.TokenHash.Length != tokenHash.Length ||
                         !CryptographicOperations.FixedTimeEquals(member.TokenHash, tokenHash))
                {
                    rejection = "token_mismatch";
                }
                if (!string.IsNullOrEmpty(rejection))
                {
                    return new EraTeamChange<bool>
                    {
                        Result = false
                    };
                }
                endpointChanged = member!.Address != endpoint.Address.ToString() || member.Port != endpoint.Port;

                member.Address = endpoint.Address.ToString();
                member.Port = endpoint.Port;
                var expiration = now.AddSeconds(_setting.EndpointLifetimeSeconds);
                if (expiration > member.TicketExpiresAt)
                {
                    expiration = member.TicketExpiresAt;
                }

                member.EndpointExpiresAt = expiration;
                return new EraTeamChange<bool>
                {
                    Changed = true,
                    Team = team,
                    Result = true
                };
            }, cancellationToken).ConfigureAwait(false);
            if (!registered)
            {
                LogRejected(teamId, memberId, endpoint, rejection);
            }
            else if (endpointChanged)
            {
                _logger.Information("ERA UDP registration accepted: team {TeamId}, member {MemberId}, endpoint {Endpoint}", teamId, memberId, endpoint);
            }
        }
        catch (EraTeamWriteException exception)
        {
            LogRejected(teamId, memberId, endpoint, "redis_commit_" + exception.Status);
            // 并发冲突或成员失效时丢弃本次登记，下次客户端登记重新校验，不能回写旧凭证。
        }
    }

    private void LogRejected(Guid teamId, Guid memberId, IPEndPoint endpoint, string reason)
    {
        if (_rejectionWindow.Elapsed.TotalSeconds < 5)
        {
            return;
        }
        _rejectionWindow.Restart();
        _logger.Warning("ERA UDP registration rejected: reason {Reason}, team {TeamId}, member {MemberId}, endpoint {Endpoint}", reason, teamId, memberId, endpoint);
    }

}
