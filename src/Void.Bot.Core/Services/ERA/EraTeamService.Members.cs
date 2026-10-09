using System.Net;
using StackExchange.Redis;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraTeamService
{
    Task<TransferEraTeamMemberResponse> TransferHostAsync(TransferEraTeamMemberCommand message, CancellationToken cancellationToken);
    Task<RemoveEraTeamMemberResponse> RemoveMemberAsync(RemoveEraTeamMemberCommand message, CancellationToken cancellationToken);
}

public partial class EraTeamService
{
    public Task<TransferEraTeamMemberResponse> TransferHostAsync(TransferEraTeamMemberCommand message, CancellationToken cancellationToken)
    {
        return ManageMemberAsync<TransferEraTeamMemberResponse>(message.TeamId, message.MemberId,
            message.TargetMemberId, message.AuthorityEpoch, true, cancellationToken);
    }

    public Task<RemoveEraTeamMemberResponse> RemoveMemberAsync(RemoveEraTeamMemberCommand message, CancellationToken cancellationToken)
    {
        return ManageMemberAsync<RemoveEraTeamMemberResponse>(message.TeamId, message.MemberId,
            message.TargetMemberId, message.AuthorityEpoch, false, cancellationToken);
    }

    private async Task<T> ManageMemberAsync<T>(Guid teamId, Guid memberId, Guid targetMemberId,
        long authorityEpoch, bool transfer, CancellationToken cancellationToken)
        where T : EraResponse<EraTeamDto>, new()
    {
        try
        {
            var identity = await IdentifyAsync<T>(cancellationToken).ConfigureAwait(false);
            if (identity.Error != null)
            {
                return identity.Error;
            }
            var initial = await _teams.GetTeamAsync(teamId, cancellationToken).ConfigureAwait(false);
            var now = DateTimeOffset.UtcNow;
            var actor = initial?.Members.SingleOrDefault(member => member.MemberId == memberId
                && member.PlayerId == identity.PlayerId!.Value && member.ExpiresAt > now);
            if (initial == null || !IsActive(initial, now) || actor == null)
            {
                return Error<T>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
            }
            if (!await OwnsCharacterAsync(identity.PlayerId!.Value, actor.CharacterId, cancellationToken).ConfigureAwait(false))
            {
                return Error<T>(HttpStatusCode.NotFound, "era.character_not_found", text => text.CharacterNotFound);
            }

            // 身份、队长权限和目标会话在每次 CAS 重试中重新检查；拒绝请求不写队伍。
            return await ChangeTeamAsync<T>(teamId, team =>
            {
                var currentTime = DateTimeOffset.UtcNow;
                T Reject(HttpStatusCode status, string code,
                    System.Linq.Expressions.Expression<Func<Void.Bot.Core.Resources.i18n.ERA.EraContextTemplate, object>> text)
                {
                    return Error<T>(status, code, text);
                }
                EraTeamChange<T> Unchanged(T result)
                {
                    return new EraTeamChange<T>
                    {
                        Result = result
                    };
                }
                var currentActor = team?.Members.SingleOrDefault(member => member.MemberId == memberId
                    && member.PlayerId == identity.PlayerId.Value && member.ExpiresAt > currentTime);
                if (team == null || !IsActive(team, currentTime) || currentActor == null
                    || currentActor.CharacterId != actor.CharacterId)
                {
                    return Unchanged(Reject(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound));
                }
                if (team.HostPlayerId != identity.PlayerId.Value)
                {
                    return Unchanged(Reject(HttpStatusCode.Forbidden, "era.team_host_required", text => text.TeamHostRequired));
                }
                if (team.IsMigrating)
                {
                    return Unchanged(Reject(HttpStatusCode.Conflict, "era.team_migrating", text => text.TeamMigrating));
                }
                if (team.AuthorityEpoch != authorityEpoch)
                {
                    return Unchanged(Reject(HttpStatusCode.Conflict, "era.authority_changed", text => text.AuthorityChanged));
                }
                var target = team.Members.SingleOrDefault(member => member.MemberId == targetMemberId
                    && member.ExpiresAt > currentTime && member.PlayerId != identity.PlayerId.Value);
                if (target == null)
                {
                    return Unchanged(Reject(HttpStatusCode.NotFound, "era.team_member_not_found", text => text.TeamMemberNotFound));
                }
                if (transfer && !target.CanHost)
                {
                    return Unchanged(Reject(HttpStatusCode.Conflict, "era.team_host_not_ready", text => text.TeamHostNotReady));
                }

                if (transfer)
                {
                    // 保留原队长为普通成员；新主机通过现有迁移确认接口恢复世界。
                    team.HostPlayerId = target.PlayerId;
                    team.AuthorityEpoch = checked(team.AuthorityEpoch + 1);
                    team.IsMigrating = true;
                    team.MigrationDeadline = currentTime.AddSeconds(_setting.MigrationTimeoutSeconds);
                    team.FailedHostMembers.Clear();
                    foreach (var member in team.Members)
                    {
                        member.AttemptId = Guid.Empty;
                        member.TokenHash = Array.Empty<byte>();
                        member.TicketExpiresAt = default;
                        member.Address = null;
                        member.Port = 0;
                        member.LocalAddresses.Clear();
                        member.LocalPort = 0;
                        member.EndpointExpiresAt = default;
                    }
                }
                else
                {
                    team.Members.RemoveAll(member => member.MemberId == targetMemberId);
                    team.FailedHostMembers.Remove(targetMemberId);
                }
                return new EraTeamChange<T>
                {
                    Changed = true,
                    Team = team,
                    Result = new T
                    {
                        Data = MapTeam(team, currentTime)
                    }
                };
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<T>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }
}
