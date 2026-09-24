using Microsoft.Extensions.Configuration;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Settings.System
{
    public class CacheSetting(IConfiguration configuration) : IConfigurationSetting
    {
        public CacheTypeEnum UseStorage {  get; } = configuration.GetValue<CacheTypeEnum>("Cache:UseStorage");
        public string RedisConnection { get; } = configuration["ConnectionStrings:Redis"] ?? string.Empty;
        public int RedisDatabaseIndex { get; } = configuration.GetValue<int?>("Cache:RedisDatabaseIndex") ?? 0;
    }
}