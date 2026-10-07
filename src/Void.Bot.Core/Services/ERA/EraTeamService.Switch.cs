using System.Net;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraTeamService
{
    Task<SwitchEraTeamResponse> SwitchAsync(SwitchEraTeamCommand message, CancellationToken cancellationToken);
}

public partial class EraTeamService
{
    public async Task<SwitchEraTeamResponse> SwitchAsync(SwitchEraTeamCommand message, CancellationToken cancellationToken)
    {
        try
        {
            var identity = await IdentifyAsync<SwitchEraTeamResponse>(cancellationToken).ConfigureAwait(false);
            if (identity.Error != null)
            {
                return identity.Error;
            }
            if (!await OwnsCharacterAsync(identity.PlayerId!.Value, message.CharacterId, cancellationToken).ConfigureAwait(false))
            {
                return Error<SwitchEraTeamResponse>(HttpStatusCode.NotFound, "era.character_not_found", text => text.CharacterNotFound);
            }
            var code = message.TeamCode.ToUpperInvariant();
            for (var attempt = 0; attempt < 8; attempt++)
            {
                var targetId = await _teams.GetTeamIdByCodeAsync(code, cancellationToken).ConfigureAwait(false);
                if (!targetId.HasValue)
                {
                    return Error<SwitchEraTeamResponse>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
                }
                var target = await _teams.GetTeamAsync(targetId.Value, cancellationToken).ConfigureAwait(false);
                var original = await _teams.GetTeamAsync(message.TeamId, cancellationToken).ConfigureAwait(false);
                var now = DateTimeOffset.UtcNow;
                if (target == null || target.TeamCode != code || !IsActive(target, now))
                {
                    return Error<SwitchEraTeamResponse>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
                }
                var alreadyJoined = target.Members.SingleOrDefault(member => member.PlayerId == identity.PlayerId.Value && member.ExpiresAt > now);
                if (alreadyJoined != null && alreadyJoined.CharacterId == message.CharacterId)
                {
                    // 切换结果丢失后，同一目标重试恢复现有成员，不能再次执行退出。
                    return new SwitchEraTeamResponse
                    {
                        Data = MapTeam(target, now)
                    };
                }
                if (alreadyJoined != null)
                {
                    return Error<SwitchEraTeamResponse>(HttpStatusCode.Conflict, "era.team_already_joined", text => text.TeamAlreadyJoined);
                }
                if (original == null || !IsActive(original, now) || !original.Members.Any(member =>
                    member.PlayerId == identity.PlayerId.Value && member.MemberId == message.MemberId && member.ExpiresAt > now))
                {
                    return Error<SwitchEraTeamResponse>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
                }
                if (target.IsMigrating || original.IsMigrating)
                {
                    return Error<SwitchEraTeamResponse>(HttpStatusCode.Conflict, "era.team_migrating", text => text.TeamMigrating);
                }
                if (target.Members.Count(member => member.ExpiresAt > now) >= target.MaxMembers)
                {
                    return Error<SwitchEraTeamResponse>(HttpStatusCode.Conflict, "era.team_capacity", text => text.TeamCapacity);
                }
                var expectedOriginal = original.StorageVersion;
                var expectedTarget = target.StorageVersion;
                original.Members.RemoveAll(member => member.PlayerId == identity.PlayerId.Value);
                var alive = EraTeamLifecycle.Advance(original, now, _setting.MigrationTimeoutSeconds);
                target.Members.RemoveAll(member => member.ExpiresAt <= now);
                target.Members.Add(SetMemberAccount(new EraTeamMember
                {
                    MemberId = Guid.NewGuid(),
                    PlayerId = identity.PlayerId.Value,
                    CharacterId = message.CharacterId,
                    JoinedAt = now,
                    ExpiresAt = now.AddSeconds(_setting.MemberLifetimeSeconds)
                }));
                var status = await _teams.TrySwitchAsync(original, expectedOriginal, alive ? original : null,
                    target, expectedTarget, identity.PlayerId.Value, cancellationToken).ConfigureAwait(false);
                if (status == EraTeamWriteStatus.Success)
                {
                    return new SwitchEraTeamResponse
                    {
                        Data = MapTeam(target, now)
                    };
                }
                if (status == EraTeamWriteStatus.PlayerAlreadyJoined)
                {
                    return Error<SwitchEraTeamResponse>(HttpStatusCode.Conflict, "era.team_already_joined", text => text.TeamAlreadyJoined);
                }
            }
            return Error<SwitchEraTeamResponse>(HttpStatusCode.Conflict, "era.version_conflict", text => text.VersionConflict);
        }
        catch (RedisException)
        {
            return Error<SwitchEraTeamResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }
}
