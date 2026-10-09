using StackExchange.Redis;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraTeamDataProvider
{
    Task<List<Guid>> GetOnlineAccountIdsAsync(CancellationToken cancellationToken);
    Task<int> CheckClientSessionAsync(Guid accountId, Guid clientId, bool acquire, CancellationToken cancellationToken);
    Task<bool> IsClientSessionActiveAsync(Guid accountId, Guid? clientId, CancellationToken cancellationToken);
    Task ReleaseClientSessionAsync(Guid accountId, Guid clientId, CancellationToken cancellationToken);
}

public partial class EraTeamDataProvider
{
    // 单个账号的检查、获取和续期在 Redis 中原子完成，支持多个 API 实例。
    private const string ClientSessionScript = """
        local owner = redis.call('GET', KEYS[1])
        if owner and owner ~= ARGV[1] then
            return -1
        end
        if not owner and ARGV[2] ~= '1' then
            return 0
        end
        redis.call('SET', KEYS[1], ARGV[1], 'EX', 30)
        local time = redis.call('TIME')
        local now = tonumber(time[1]) * 1000 + math.floor(tonumber(time[2]) / 1000)
        redis.call('ZADD', KEYS[2], now + 30000, ARGV[3])
        return 1
        """;
    private const string ReleaseClientSessionScript = """
        if redis.call('GET', KEYS[1]) == ARGV[1] then
            redis.call('ZREM', KEYS[2], ARGV[2])
            return redis.call('DEL', KEYS[1])
        end
        return 0
        """;

    public async Task<int> CheckClientSessionAsync(Guid accountId, Guid clientId, bool acquire, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var result = await database.ScriptEvaluateAsync(ClientSessionScript,
            new RedisKey[] { $"era:client-session:v1:{accountId:N}", "era:online-accounts:v1" },
            new RedisValue[] { clientId.ToString("N"), acquire ? "1" : "0", accountId.ToString("D") }).WaitAsync(cancellationToken).ConfigureAwait(false);
        return (int)result;
    }

    public async Task<List<Guid>> GetOnlineAccountIdsAsync(CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        const string script = """
            local time = redis.call('TIME')
            local now = tonumber(time[1]) * 1000 + math.floor(tonumber(time[2]) / 1000)
            redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', now)
            return redis.call('ZRANGE', KEYS[1], 0, -1)
            """;
        var result = await database.ScriptEvaluateAsync(script,
            new RedisKey[] { "era:online-accounts:v1" }).WaitAsync(cancellationToken).ConfigureAwait(false);
        return ((RedisResult[])result!).Select(value => Guid.Parse(value.ToString())).ToList();
    }

    public async Task<bool> IsClientSessionActiveAsync(
        Guid accountId, Guid? clientId, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var value = await database.StringGetAsync($"era:client-session:v1:{accountId:N}")
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!value.HasValue)
        {
            return false;
        }
        if (!clientId.HasValue)
        {
            return true;
        }
        return value.ToString() == clientId.Value.ToString("N");
    }

    public async Task ReleaseClientSessionAsync(Guid accountId, Guid clientId, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await database.ScriptEvaluateAsync(ReleaseClientSessionScript,
            new RedisKey[] { $"era:client-session:v1:{accountId:N}", "era:online-accounts:v1" },
            new RedisValue[] { clientId.ToString("N"), accountId.ToString("D") }).WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}
