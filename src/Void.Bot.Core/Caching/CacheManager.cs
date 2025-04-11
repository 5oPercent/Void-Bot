using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Void.Bot.Core.Settings;

namespace Void.Bot.Core.Caching
{
    public class CacheManager : ICacheManager
    {
        private readonly ICacheManager _cacheManager;

        public CacheManager(CacheSetting cacheSetting,IConnectionMultiplexer redisConnection,IMemoryCache memoryCache)
        {
            if (cacheSetting.cacheType == CacheType.Redis)
            {
                _cacheManager = new RedisCacheManager(redisConnection, cacheSetting.RedisConnection, cacheSetting.RedisDatabaseIndex);
            }
            else
            {
                _cacheManager = new MemoryCacheManager(memoryCache);
            }
        }

        public Task<T?> Get<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            return _cacheManager.Get<T>(key, cancellationToken);
        }

        public Task Set(string key, object data, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            return _cacheManager.Set(key, data, expiry, cancellationToken);
        }

        public Task Remove(string key, CancellationToken cancellationToken = default)
        {
            return _cacheManager.Remove(key, cancellationToken);
        }

        public Task RemoveByPrefix(string pattern, CancellationToken cancellationToken = default)
        {
            return _cacheManager.RemoveByPrefix(pattern, cancellationToken);
        }

        public Task Clear(CancellationToken cancellationToken = default)
        {
            return _cacheManager.Clear(cancellationToken);
        }

        public void Dispose()
        {
            _cacheManager.Dispose();
        }
    }
}
