using Void.Bot.Core.Settings.Bungie;

namespace Void.Bot.Core.Validators.Bungie.Destiny2;

internal static class Destiny2WeaponLanguageValidation
{
    public static bool IsSupported(string? language, Destiny2Setting setting)
    {
        return string.IsNullOrWhiteSpace(language) || setting.SupportedLanguages.Contains(language.Trim());
    }
}
