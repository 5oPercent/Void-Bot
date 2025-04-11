using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Void.Bot.Core.Settings
{
    public class CacheSetting
    {
        public CacheType cacheType {  get; set; } = CacheType.Memory;
        public string RedisConnection { get; set; } = "localhost:6379";
        public int RedisDatabaseIndex { get; set; } = 0;
    }
    public enum CacheType
    {
        Memory,
        Redis
    }
}
