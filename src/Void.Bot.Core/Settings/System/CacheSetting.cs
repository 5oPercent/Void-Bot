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
            CacheType = configuration.GetValue<CacheTypeEnum>("CacheSetting:CacheType");
            RedisConnection = configuration["ConnectionStrings:Redis"];
            RedisDatabaseIndex = configuration.GetValue<int?>("CacheSetting:RedisDatabaseIndex") ?? 0;
        }
    }
}
