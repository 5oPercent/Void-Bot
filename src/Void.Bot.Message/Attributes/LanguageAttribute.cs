using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Attributes;

/// <summary>
/// 语言特性标记, 自动在i18n文件夹中输出属性内容的yaml配置文件.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LanguageAttribute(string system, string fileName) : Attribute
{
    public string System { get; set; } = system;
    
    public string FileName { get; set; } = fileName;
    
    public LanguageEnum Language { get; set; } = LanguageEnum.Default;
    
    public string Key => $"/{System}/{FileName}.{Language}.yaml";
}