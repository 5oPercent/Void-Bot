using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Attributes;

/// <summary>
/// 语言特性标记, 自动在i18n文件夹中输出属性内容的yaml配置文件.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LanguageAttribute(string system, string contextTemplateName) : Attribute
{
    public string System { get; set; } = system;
    
    public string ContextTemplateName { get; set; } = contextTemplateName;
    
    public LanguageEnum Language { get; set; } = LanguageEnum.Default;
    
    public string Key => $"/{System}/{ContextTemplateName}.{Language}.yaml";
}