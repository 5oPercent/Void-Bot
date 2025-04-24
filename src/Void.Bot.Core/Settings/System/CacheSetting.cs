using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Settings.System
{
    public class CacheSetting : IConfigurationSetting
    {
        public CacheTypeEnum CacheType {  get; set; } 
        public string RedisConnection { get; set; } 
        public int RedisDatabaseIndex { get; set; }

        public CacheSetting(IConfiguration configuration)
        {
            var section = configuration.GetSection("CacheSetting");
            Enum.TryParse(section["CacheType"], out CacheTypeEnum cacheType);
            CacheType = cacheType;

            RedisConnection = section["RedisConnection"] ?? "localhost:6379";
            RedisDatabaseIndex = int.TryParse(section["RedisDatabaseIndex"], out var index) ? index : 0;
        }
    }
}
