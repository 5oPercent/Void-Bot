namespace Void.Bot.Message.Attributes;

/// <summary>
/// Skips endpoint permission verification while retaining authentication.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class SkipPermissionCheckAttribute : Attribute
{
}