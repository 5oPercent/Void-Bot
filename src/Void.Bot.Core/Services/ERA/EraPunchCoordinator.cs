using System.Net;
using System.Security.Cryptography;
using System.Text;
using Void.Bot.Core.Settings.ERA;

namespace Void.Bot.Core.Services.ERA;

public interface IEraPunchCoordinator : ISingleton
{
    Task RemoveExpiredAsync(CancellationToken cancellationToken);
    Task RegisterAsync(Guid teamId, Guid memberId, Guid attemptId, string token, IPEndPoint endpoint, CancellationToken cancellationToken);
}

public class EraPunchCoordinator : IEraPunchCoordinator
{
    private readonly IEraTeamDataProvider _dataProvider;
    private readonly EraNetworkingSetting _setting;

    public EraPunchCoordinator(IEraTeamDataProvider dataProvider, EraNetworkingSetting setting)
    {
        _dataProvider = dataProvider;
        _setting = setting;
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
            return;
        }

        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        try
        {
            await _dataProvider.ChangeAsync(teamId, team =>
            {
                var now = DateTimeOffset.UtcNow;
                if (team == null || team.ExpiresAt <= now ||
                    !team.Members.Any(item => item.ExpiresAt > now))
                {
                    return new EraTeamChange<bool>
                    {
                        Result = false
                    };
                }

                var member = team.Members.SingleOrDefault(item => item.MemberId == memberId);
                if (member == null || member.ExpiresAt <= now || member.TicketAuthorityEpoch != team.AuthorityEpoch || member.AttemptId != attemptId ||
                    member.TicketExpiresAt <= now || member.TokenHash.Length != tokenHash.Length ||
                    !CryptographicOperations.FixedTimeEquals(member.TokenHash, tokenHash))
                {
                    return new EraTeamChange<bool>
                    {
                        Result = false
                    };
                }

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
        }
        catch (EraTeamWriteException)
        {
            // 并发冲突或成员失效时丢弃本次登记，下次客户端登记重新校验，不能回写旧凭证。
        }
    }
}
