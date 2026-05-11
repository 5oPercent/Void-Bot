using System.Reflection;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Extension;

public static class LanguageExtension
{
    public static string GetYamlKey<T>(this T contextTemplate, LanguageEnum language = LanguageEnum.Default) where T : IContextTemplate
    {
        var type = contextTemplate.GetType();
        var att = type.GetCustomAttribute<LanguageAttribute>();
        if (att == null)
            throw new InvalidOperationException($"Class {type.Name} is not marked with LanguageAttribute");

        return att.GetYamlKey(language);
    }
    
    private static string GetYamlKey(this LanguageAttribute attribute, LanguageEnum language)
    {
        var targetLanguage = language == LanguageEnum.Default ? attribute.Language : language;
        return $"/{attribute.System}/{attribute.ContextTemplateName}.{targetLanguage.ToI18nCode()}.yaml";
    }
    
    public static LanguageEnum MapI18nCodeToEnum(string? language)
    {
        return language?.Trim().ToLowerInvariant() switch
        {
            "zh-chs" => LanguageEnum.ZhChs,
            "zh-cht" => LanguageEnum.ZhCht,
            "en" => LanguageEnum.En,
            _ => LanguageEnum.Default
        };
    }

    public static string ToI18nCode(this LanguageEnum language)
    {
        return language switch
        {
            LanguageEnum.ZhChs => "zh-chs",
            LanguageEnum.ZhCht => "zh-cht",
            LanguageEnum.En => "en",
            _ => "zh-chs"
        };
    }
}
