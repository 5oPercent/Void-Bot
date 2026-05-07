using Void.Bot.Message.Enum;
using Void.Bot.Message.Extension;

namespace Void.Bot.Message.Attributes;

/// <summary>
/// 语言特性标记, 自动在i18n文件夹中输出属性内容的yaml配置文件.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LanguageAttribute(
    string system,
    string contextTemplateName,
    LanguageEnum language = LanguageEnum.Default) : Attribute
{
    public string System { get; } = system;
    
    public string ContextTemplateName { get; } = contextTemplateName;
    
    public LanguageEnum Language { get; } = language;
    
    public string Key => $"/{System}/{ContextTemplateName}.{Language.ToLanguageCode()}.yaml";
}
