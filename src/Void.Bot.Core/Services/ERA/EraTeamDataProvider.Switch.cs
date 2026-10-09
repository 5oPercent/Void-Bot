using System.Text.Json;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Constants.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraTeamDataProvider
{
    Task<EraTeamWriteStatus> TrySwitchAsync(EraTeam original, Guid expectedOriginalVersion,
        EraTeam? remaining, EraTeam target, Guid expectedTargetVersion, Guid movingPlayerId,
        CancellationToken cancellationToken);
}

public partial class EraTeamDataProvider
{
    private const string SwitchScript = SocialCommitFunctions + "\n" + """
        local original = redis.call('GET', KEYS[2])
        local target = redis.call('GET', KEYS[4])
        if not original or not target then
            return 0
        end
        if cjson.decode(original).StorageVersion ~= ARGV[1] or
           cjson.decode(target).StorageVersion ~= ARGV[2] then
            return 0
        end
        local time = redis.call('TIME')
        local now = tonumber(time[1]) * 1000 + math.floor(tonumber(time[2]) / 1000)
        local teams = cjson.decode(ARGV[5])
        for _, team in ipairs(teams) do
            if team.Json ~= '' then
                if team.Expiry <= now then
                    return -3
                end
                local codeOwner = redis.call('GET', KEYS[team.CodeKey])
                if codeOwner and codeOwner ~= team.Id then
                    return -4
                end
                for _, lease in ipairs(team.Leases) do
                    if lease.Expiry > now then
                        local owner = redis.call('GET', KEYS[lease.Key])
                        if owner and owner ~= team.Id and
                           not (lease.Key == tonumber(ARGV[6]) and owner == ARGV[3]) then
                            return -2
                        end
                    end
                end
            end
        end
        for _, team in ipairs(teams) do
            local before = team.TeamKey == 2 and cjson.decode(original) or cjson.decode(target)
            local after = team.Json ~= '' and cjson.decode(team.Json) or nil
            if not validSocialTransition(before, after, now) then
                return -3
            end
        end
        for i = 6, #KEYS do
            local owner = redis.call('GET', KEYS[i])
            if owner == ARGV[3] or owner == ARGV[4] then
                redis.call('DEL', KEYS[i])
            end
        end
        for _, team in ipairs(teams) do
            if team.Json == '' then
                redis.call('DEL', KEYS[team.TeamKey])
                if redis.call('GET', KEYS[team.CodeKey]) == team.Id then
                    redis.call('DEL', KEYS[team.CodeKey])
                end
                redis.call('ZREM', KEYS[1], team.Id)
            else
                redis.call('SET', KEYS[team.TeamKey], team.Json, 'PXAT', team.Expiry)
                redis.call('SET', KEYS[team.CodeKey], team.Id, 'PXAT', team.Expiry)
                redis.call('ZADD', KEYS[1], team.Expiry, team.Id)
                for _, lease in ipairs(team.Leases) do
                    if lease.Expiry > now then
                        redis.call('SET', KEYS[lease.Key], team.Id, 'PXAT', lease.Expiry)
                    end
                end
            end
        end
        for _, team in ipairs(teams) do
            local before = team.TeamKey == 2 and cjson.decode(original) or cjson.decode(target)
            local after = team.Json ~= '' and cjson.decode(team.Json) or nil
            publishInvitations(before, after, team.Id, now)
        end
        return 1
        """;

    public async Task<EraTeamWriteStatus> TrySwitchAsync(EraTeam original, Guid expectedOriginalVersion,
        EraTeam? remaining, EraTeam target, Guid expectedTargetVersion, Guid movingPlayerId,
        CancellationToken cancellationToken)
    {
        var players = original.Members.Select(member => member.PlayerId)
            .Concat(target.Members.Select(member => member.PlayerId)).Distinct().ToList();
        // original 可能已经移除移动成员，必须仍把其所属关系纳入同一原子脚本。
        if (!players.Contains(movingPlayerId))
        {
            players.Add(movingPlayerId);
        }
        var keys = new List<RedisKey>
        {
            EraNetworkingConstants.RedisTeamIndex,
            TeamKey(original.TeamId),
            EraNetworkingConstants.RedisTeamCodePrefix + original.TeamCode,
            TeamKey(target.TeamId),
            EraNetworkingConstants.RedisTeamCodePrefix + target.TeamCode
        };
        keys.AddRange(players.Select(PlayerKey));
        if (remaining != null)
        {
            remaining.StorageVersion = Guid.NewGuid();
        }
        target.StorageVersion = Guid.NewGuid();
        var teams = new List<SwitchTeam>
        {
            DescribeSwitchTeam(original.TeamId, remaining, 2, 3, players),
            DescribeSwitchTeam(target.TeamId, target, 4, 5, players)
        };
        var values = new RedisValue[]
        {
            expectedOriginalVersion.ToString("D"),
            expectedTargetVersion.ToString("D"),
            original.TeamId.ToString("D"),
            target.TeamId.ToString("D"),
            JsonSerializer.Serialize(teams),
            players.IndexOf(movingPlayerId) + 6,
            DateTimeOffset.UtcNow.ToString("O"),
            Guid.NewGuid().ToString("D")
        };
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var result = await database.ScriptEvaluateAsync(SwitchScript, keys.ToArray(), values).ConfigureAwait(false);
        return (EraTeamWriteStatus)(int)result;
    }

    private static SwitchTeam DescribeSwitchTeam(Guid id, EraTeam? team, int teamKey, int codeKey, List<Guid> players)
    {
        var result = new SwitchTeam
        {
            Id = id.ToString("D"),
            TeamKey = teamKey,
            CodeKey = codeKey
        };
        if (team == null)
        {
            return result;
        }
        result.Expiry = Math.Min(team.ExpiresAt.ToUnixTimeMilliseconds(), team.Members.Max(member => member.ExpiresAt.ToUnixTimeMilliseconds()));
        PrepareInvitations(team);
        result.Json = JsonSerializer.Serialize(team);
        foreach (var member in team.Members)
        {
            result.Leases.Add(new SwitchLease
            {
                Key = players.IndexOf(member.PlayerId) + 6,
                Expiry = Math.Min(result.Expiry, member.ExpiresAt.ToUnixTimeMilliseconds())
            });
        }
        return result;
    }

    private class SwitchTeam
    {
        public string Id { get; set; } = string.Empty;
        public string Json { get; set; } = string.Empty;
        public int TeamKey { get; set; }
        public int CodeKey { get; set; }
        public long Expiry { get; set; }
        public List<SwitchLease> Leases { get; set; } = new List<SwitchLease>();
    }

    private class SwitchLease
    {
        public int Key { get; set; }
        public long Expiry { get; set; }
    }
}
