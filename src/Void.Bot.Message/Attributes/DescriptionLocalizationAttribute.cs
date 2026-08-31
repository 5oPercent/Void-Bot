namespace Void.Bot.Message.Attributes;

/// <summary>
/// Specifies the stable i18n key used to localize a description.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class DescriptionLocalizationAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
