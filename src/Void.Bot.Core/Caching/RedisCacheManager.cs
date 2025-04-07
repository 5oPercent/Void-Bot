using Hangfire.MemoryStorage.Database;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Void.Bot.Core.Caching
{
    public class RedisCacheManager : ICacheManager
    {
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly IDatabase _database;

        public RedisCacheManager(string connection)
        {
            _redisConnection = ConnectionMultiplexer.Connect(connection);
            _database = _redisConnection.GetDatabase();
        }

        public byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return Array.Empty<byte>();
            }
            return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        public async Task<T?> Get<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var redisValue = await _database.StringGetAsync(key);
            if(!redisValue.HasValue)
            {
                return null;
            }
            return System.Text.Json.JsonSerializer.Deserialize<T>(redisValue);
        }

        public async Task Set(string key, object data, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            var value = Serialize(data);
            await _database.StringSetAsync(key, value, expiry);
        }

        public async Task Remove(string key, CancellationToken cancellationToken = default)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task RemoveByPrefix(string pattern, CancellationToken cancellationToken = default)
        {
            var endpoints = _redisConnection.GetEndPoints();
            var server = _redisConnection.GetServer(endpoints[0]);

            var keys = server.Keys(pattern: pattern);
            foreach(var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }
        }

        public async Task Clear(CancellationToken cancellationToken = default)
        {
            var endpoints = _redisConnection.GetEndPoints();
            var server = _redisConnection.GetServer(endpoints[0]);

            foreach(var key in server.Keys())
            {
                await _database.KeyDeleteAsync(key);
            }
        }

        public void Dispose()
        {
            _redisConnection.Dispose();
        }
    }
}
