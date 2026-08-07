using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Bungie;

public class Destiny2Setting : IConfigurationSetting
{
    public List<string> SyncLanguages { get; }
    
    public List<string> SyncManifests { get; }

    public int CachePlayerDataMinutes { get; }

    public Destiny2Setting(IConfiguration configuration)
    {
        SyncLanguages = configuration.GetSection("Destiny2:SyncLanguages").Get<List<string>>() ?? [];
        SyncManifests = configuration.GetSection("Destiny2:SyncManifests").Get<List<string>>() ?? [];
        CachePlayerDataMinutes = configuration.GetSection("Destiny2:Cache:PlayerDataMinutes").Get<int?>() ?? 3;
    }
}