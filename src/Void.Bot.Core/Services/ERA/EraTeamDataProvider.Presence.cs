using System.Text.Json;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA;

public class EraPresenceSnapshot
{
    public Guid AccountId { get; set; }
    public bool IsOnline { get; set; }
    public EraTeam? Team { get; set; }

    public string Fingerprint()
    {
        if (!IsOnline)
        {
            return "offline";
        }
        if (Team == null)
        {
            return "online";
        }
        var members = Team.Members.Where(value => value.ExpiresAt > DateTimeOffset.UtcNow)
            .Select(value => value.MemberId).OrderBy(value => value).ToArray();
        return string.Join("/", Team.TeamId, Team.HostPlayerId, Team.JoinMode, Team.InvitationPermission,
            Team.SocialVersion, Team.MaxMembers, Team.IsMigrating, string.Join(",", members));
    }
}

public partial interface IEraTeamDataProvider
{
    Task<List<EraPresenceSnapshot>> GetPresenceSnapshotsAsync(IReadOnlyCollection<Guid> accounts,
        IReadOnlyDictionary<Guid, Guid> players, CancellationToken cancellationToken);
}

public partial class EraTeamDataProvider
{
    public async Task<List<EraPresenceSnapshot>> GetPresenceSnapshotsAsync(IReadOnlyCollection<Guid> accounts,
        IReadOnlyDictionary<Guid, Guid> players, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var ids = accounts.ToArray();
        if (ids.Length == 0)
        {
            return new List<EraPresenceSnapshot>();
        }
        var sessions = await database.StringGetAsync(ids.Select(value =>
            (RedisKey)$"era:client-session:v1:{value:N}").ToArray())
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        var playerIds = ids.Select(value => players.TryGetValue(value, out var player) ? player : Guid.Empty).ToArray();
        var teamIds = await database.StringGetAsync(playerIds.Select(PlayerKey).ToArray())
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        var uniqueTeams = teamIds.Where(value => value.HasValue).Select(value => Guid.Parse(value.ToString()))
            .Distinct().ToArray();
        var teams = new Dictionary<Guid, EraTeam>();
        if (uniqueTeams.Length > 0)
        {
            var values = await database.StringGetAsync(uniqueTeams.Select(TeamKey).ToArray())
                .WaitAsync(cancellationToken).ConfigureAwait(false);
            for (var index = 0; index < values.Length; index++)
            {
                if (values[index].HasValue)
                {
                    var team = JsonSerializer.Deserialize<EraTeam>(values[index].ToString());
                    if (team != null && team.ExpiresAt > DateTimeOffset.UtcNow)
                    {
                        teams[uniqueTeams[index]] = team;
                    }
                }
            }
        }
        var result = new List<EraPresenceSnapshot>();
        for (var index = 0; index < ids.Length; index++)
        {
            EraTeam? team = null;
            if (sessions[index].HasValue && teamIds[index].HasValue &&
                teams.TryGetValue(Guid.Parse(teamIds[index].ToString()), out var candidate) &&
                candidate.Members.Any(value => value.PlayerId == playerIds[index] && value.ExpiresAt > DateTimeOffset.UtcNow))
            {
                team = candidate;
            }
            result.Add(new EraPresenceSnapshot
            {
                AccountId = ids[index],
                IsOnline = sessions[index].HasValue,
                Team = team
            });
        }
        return result;
    }
}
