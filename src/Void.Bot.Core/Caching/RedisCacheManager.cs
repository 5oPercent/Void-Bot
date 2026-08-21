using System.Buffers;
using System.Text.Json;
using StackExchange.Redis;
using Void.Bot.Core.Extension;

namespace Void.Bot.Core.Caching
{
    public class RedisCacheManager : ICacheManager
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

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
            return redisValue.HasValue ? DeserializeValue<T>(redisValue) : null;
        }

        public async Task<T?> Take<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var result = await _database.ScriptEvaluateAsync(
                    TakeScript,
                    new RedisKey[] { key },
                    Array.Empty<RedisValue>())
                .WaitAsync(cancellationToken);

            return result.IsNull ? null : DeserializeValue<T>((RedisValue)result);
        }

        public async Task Set(string key, object data, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            // JSON manifest components are already serialized strings and can be hundreds of MB.
            // Serializing them again creates one enormous JSON string token, which exceeds
            // System.Text.Json's supported token length. Store strings directly instead.
            if (data is string stringValue)
            {
                await _database.StringSetAsync(key, stringValue, expiry);
                return;
            }

            // Serialize into an unpooled buffer so large derived cache payloads do not remain
            // rooted by ArrayPool<byte>.Shared after the Redis write has completed.
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                JsonSerializer.Serialize(writer, data, data.GetType());
            }

            await _database.StringSetAsync(key, (RedisValue)buffer.WrittenMemory, expiry);
        }

        private static T? DeserializeValue<T>(RedisValue value) where T : class
        {
            if (typeof(T) != typeof(string))
            {
                ReadOnlyMemory<byte> utf8Json = value;
                return JsonSerializer.Deserialize<T>(utf8Json.Span, SerializerOptions);
            }

            var stringValue = value.ToString();

            // Keep string values written by older versions readable. Previously all strings
            // were JSON serialized, so their Redis representation starts with a quote.
            if (stringValue.Length >= 2 && stringValue[0] == '"' && stringValue[^1] == '"')
            {
                try
                {
                    return (T?)(object?)stringValue.DeserializeJson<string>();
                }
                catch (System.Text.Json.JsonException)
                {
                    // A raw string may legitimately start and end with quotes.
                }
            }

            return (T)(object)stringValue;
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
