using System.Net;
using StackExchange.Redis;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraTeamService
{
    Task<SetEraHostReadinessResponse> SetHostReadinessAsync(
        SetEraHostReadinessCommand message,
        CancellationToken cancellationToken);
    Task<CompleteEraHostMigrationResponse> CompleteHostMigrationAsync(
        CompleteEraHostMigrationCommand message,
        CancellationToken cancellationToken);
}

public partial class EraTeamService
{
    public async Task<SetEraHostReadinessResponse> SetHostReadinessAsync(
        SetEraHostReadinessCommand message,
        CancellationToken cancellationToken)
    {
        try
        {
            return await InTeamAsync(message.TeamId, message.MemberId, true, (team, member, now) =>
            {
                if (message.AuthorityEpoch != team.AuthorityEpoch || team.IsMigrating)
                {
                    return Error<SetEraHostReadinessResponse>(HttpStatusCode.Conflict,
                        "era.authority_changed", text => text.AuthorityChanged);
                }
                member.CanHost = message.CanHost;
                return new SetEraHostReadinessResponse
                {
                    Data = MapTeam(team, now)
                };
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<SetEraHostReadinessResponse>(HttpStatusCode.ServiceUnavailable,
                "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }

    public async Task<CompleteEraHostMigrationResponse> CompleteHostMigrationAsync(
        CompleteEraHostMigrationCommand message,
        CancellationToken cancellationToken)
    {
        try
        {
            var identity = await IdentifyAsync<CompleteEraHostMigrationResponse>(cancellationToken).ConfigureAwait(false);
            if (identity.Error != null)
            {
                return identity.Error;
            }
            var initial = await _teams.GetTeamAsync(message.TeamId, cancellationToken).ConfigureAwait(false);
            var initialMember = initial?.Members.SingleOrDefault(member => member.MemberId == message.MemberId && member.PlayerId == identity.PlayerId!.Value);
            if (initialMember == null || !await OwnsCharacterAsync(identity.PlayerId!.Value, initialMember.CharacterId, cancellationToken).ConfigureAwait(false))
            {
                return Error<CompleteEraHostMigrationResponse>(HttpStatusCode.NotFound,
                    "era.character_not_found", text => text.CharacterNotFound);
            }
            return await ChangeTeamAsync(message.TeamId, team =>
            {
                var now = DateTimeOffset.UtcNow;
                if (team == null || !IsActive(team, now))
                {
                    return new EraTeamChange<CompleteEraHostMigrationResponse>
                    {
                        Result = Error<CompleteEraHostMigrationResponse>(HttpStatusCode.NotFound,
                            "era.team_not_found", text => text.TeamNotFound)
                    };
                }
                var member = team.Members.SingleOrDefault(item => item.MemberId == message.MemberId &&
                    item.PlayerId == identity.PlayerId!.Value && item.ExpiresAt > now);
                if (member == null || team.HostPlayerId != identity.PlayerId!.Value)
                {
                    return new EraTeamChange<CompleteEraHostMigrationResponse>
                    {
                        Result = Error<CompleteEraHostMigrationResponse>(HttpStatusCode.Forbidden,
                            "era.migration_host_required", text => text.MigrationHostRequired)
                    };
                }
                if (message.AuthorityEpoch != team.AuthorityEpoch ||
                    (team.IsMigrating && team.MigrationDeadline <= now) ||
                    (!team.IsMigrating && !message.Success))
                {
                    return new EraTeamChange<CompleteEraHostMigrationResponse>
                    {
                        Result = Error<CompleteEraHostMigrationResponse>(HttpStatusCode.Conflict,
                            "era.authority_changed", text => text.AuthorityChanged)
                    };
                }
                var alive = true;
                if (message.Success)
                {
                    team.IsMigrating = false;
                    team.MigrationDeadline = default;
                    team.FailedHostMembers.Clear();
                }
                else
                {
                    member.CanHost = false;
                    alive = EraTeamLifecycle.SelectNext(team, now, _setting.MigrationTimeoutSeconds);
                }
                return new EraTeamChange<CompleteEraHostMigrationResponse>
                {
                    Changed = true,
                    Team = alive ? team : null,
                    Result = new CompleteEraHostMigrationResponse
                    {
                        Data = alive ? MapTeam(team, now) : null
                    }
                };
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<CompleteEraHostMigrationResponse>(HttpStatusCode.ServiceUnavailable,
                "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }
}
