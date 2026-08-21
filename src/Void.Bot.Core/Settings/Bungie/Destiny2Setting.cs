using Microsoft.Extensions.Configuration;
using Void.Bot.Core.Extension;

namespace Void.Bot.Core.Settings.Bungie;

public class Destiny2Setting : IConfigurationSetting
{
    public List<string> SyncLanguages { get; }

    public IReadOnlySet<string> SupportedLanguages { get; }

    public string DefaultLanguage { get; }
    
    public List<string> SyncManifests { get; }

    public int CachePlayerDataMinutes { get; }

    public Destiny2Setting(IConfiguration configuration)
    {
        SyncLanguages = configuration.GetSection("Destiny2:SyncLanguages").Get<List<string>>() ?? [];
        var normalizedLanguages = SyncLanguages
            .Select(LanguageExtension.MapI18nCodeToEnum)
            .Select(language => language.ToI18nCode())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        SupportedLanguages = normalizedLanguages.ToHashSet(StringComparer.OrdinalIgnoreCase);
        DefaultLanguage = normalizedLanguages.Contains("zh-chs", StringComparer.OrdinalIgnoreCase)
            ? "zh-chs"
            : normalizedLanguages.FirstOrDefault() ?? "zh-chs";
        SyncManifests = configuration.GetSection("Destiny2:SyncManifests").Get<List<string>>() ?? [];
        CachePlayerDataMinutes = configuration.GetSection("Destiny2:Cache:PlayerDataMinutes").Get<int?>() ?? 3;
    }
}
