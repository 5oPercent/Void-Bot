using StackExchange.Redis;
using Void.Bot.Core.Extension;

namespace Void.Bot.Core.Caching
{
    public class RedisCacheManager : ICacheManager
    {
        private const string TakeScript = """
            local value = redis.call('GET', KEYS[1])
            if value then
                redis.call('DEL', KEYS[1])
            end
            return value
            """;

        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabase _database;

        public RedisCacheManager(IConnectionMultiplexer redisConnection, int redisDatabaseIndex)
        {
            _redisConnection = redisConnection;
            _database = redisConnection.GetDatabase(redisDatabaseIndex);
        }

        public async Task<T?> Get<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var redisValue = await _database.StringGetAsync(key);
            return redisValue.HasValue ? redisValue.ToString().DeserializeJson<T>() : null;
        }

        public async Task<T?> Take<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var result = await _database.ScriptEvaluateAsync(
                    TakeScript,
                    new RedisKey[] { key },
                    Array.Empty<RedisValue>())
                .WaitAsync(cancellationToken);

            return result.IsNull ? null : result.ToString()!.DeserializeJson<T>();
        }

        public async Task Set(string key, object data, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            await _database.StringSetAsync(key, data.SerializeJson(), expiry);
        }

        public async Task Remove(string key, CancellationToken cancellationToken = default)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> TryLockAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            return await _database.StringSetAsync(key, "locked", expiry, When.NotExists);
        }

        public async Task RemoveByPrefix(string pattern, CancellationToken cancellationToken = default)
        {
            var endpoints = _redisConnection.GetEndPoints();
            var server = _redisConnection.GetServer(endpoints[0]);

            var keys = server.Keys(pattern: pattern).ToList();
            if(keys.Count != 0)
            {
                await _database.KeyDeleteAsync(keys.ToArray());
            }
        }

        public async Task Clear(CancellationToken cancellationToken = default)
        {
            var endpoints = _redisConnection.GetEndPoints();
            var server = _redisConnection.GetServer(endpoints[0]);

            var keys = server.Keys().ToList();
            if(keys.Count != 0)
            {
                await _database.KeyDeleteAsync(keys.ToArray());
            }
        }

        public void Dispose()
        {
            _redisConnection.Dispose();
        }
    }
}