namespace Void.Bot.Core.Extension;

public static class ObjectExtension
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }
}