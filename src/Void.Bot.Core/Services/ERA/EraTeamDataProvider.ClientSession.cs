using StackExchange.Redis;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraTeamDataProvider
{
    Task<int> CheckClientSessionAsync(Guid accountId, Guid clientId, bool acquire, CancellationToken cancellationToken);
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
        return 1
        """;
    private const string ReleaseClientSessionScript = """
        if redis.call('GET', KEYS[1]) == ARGV[1] then
            return redis.call('DEL', KEYS[1])
        end
        return 0
        """;

    public async Task<int> CheckClientSessionAsync(Guid accountId, Guid clientId, bool acquire, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var result = await database.ScriptEvaluateAsync(ClientSessionScript,
            new RedisKey[] { $"era:client-session:v1:{accountId:N}" },
            new RedisValue[] { clientId.ToString("N"), acquire ? "1" : "0" }).WaitAsync(cancellationToken).ConfigureAwait(false);
        return (int)result;
    }

    public async Task ReleaseClientSessionAsync(Guid accountId, Guid clientId, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        await database.ScriptEvaluateAsync(ReleaseClientSessionScript,
            new RedisKey[] { $"era:client-session:v1:{accountId:N}" },
            new RedisValue[] { clientId.ToString("N") }).WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}
