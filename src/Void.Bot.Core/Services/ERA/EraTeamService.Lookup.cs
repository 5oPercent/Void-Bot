using System.Net;
using StackExchange.Redis;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraTeamService
{
    Task<LookupEraTeamResponse> LookupAsync(LookupEraTeamRequest request, CancellationToken cancellationToken);
}

public partial class EraTeamService
{
    public async Task<LookupEraTeamResponse> LookupAsync(LookupEraTeamRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var identity = await IdentifyAsync<LookupEraTeamResponse>(cancellationToken).ConfigureAwait(false);
            if (identity.Error != null)
            {
                return identity.Error;
            }
            string code = request.TeamCode.ToUpperInvariant();
            var id = await _teams.GetTeamIdByCodeAsync(code, cancellationToken).ConfigureAwait(false);
            var team = id.HasValue ? await _teams.GetTeamAsync(id.Value, cancellationToken).ConfigureAwait(false) : null;
            var now = DateTimeOffset.UtcNow;
            if (team == null || team.TeamCode != code || !IsActive(team, now))
            {
                return Error<LookupEraTeamResponse>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
            }
            if (!await CanJoinSocialAsync(team, null, false, cancellationToken).ConfigureAwait(false))
            {
                return Error<LookupEraTeamResponse>(HttpStatusCode.Forbidden, "era.team_join_denied", text => text.TeamJoinDenied);
            }
            if (team.IsMigrating)
            {
                return Error<LookupEraTeamResponse>(HttpStatusCode.Conflict, "era.team_migrating", text => text.TeamMigrating);
            }
            if (team.Members.Count(member => member.ExpiresAt > now) >= team.MaxMembers)
            {
                return Error<LookupEraTeamResponse>(HttpStatusCode.Conflict, "era.team_capacity", text => text.TeamCapacity);
            }
            return new LookupEraTeamResponse { Data = true };
        }
        catch (RedisException)
        {
            return Error<LookupEraTeamResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }
}
