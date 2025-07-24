using System.Reflection;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Extension;

public static class LanguageExtension
{
    public static string GetYamlKey<T>(this T contextTemplate, LanguageEnum language = LanguageEnum.Default) where T : IContextTemplate
    {
        var att = typeof(T).GetCustomAttribute<LanguageAttribute>();
        if (att == null)
            throw new InvalidOperationException($"Class {typeof(T).Name} is not marked with LanguageAttribute");

        return language == LanguageEnum.Default ? att.Key : $"/{att.System}/{att.FileName}.{language}.yaml";
    }
    
    public static string GetYamlKey(this Type type, LanguageEnum language = LanguageEnum.Default)
    {
        var att = type.GetCustomAttribute<LanguageAttribute>();
        if (att == null)
            throw new InvalidOperationException($"Class {type.Name} is not marked with LanguageAttribute");

        return language == LanguageEnum.Default ? att.Key : $"/{att.System}/{att.FileName}.{language}.yaml";
    }
}