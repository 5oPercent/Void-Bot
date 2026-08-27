namespace Void.Bot.Api.Authentication;

public abstract class ThirdPartyAuthenticationBase
{
    private const string AuthorizationPathPrefix = "/api/OAuth";

    public static bool IsAuthorizationPath(PathString path)
    {
        return TryGetPlatform(path, out _);
    }

    public static bool TryGetPlatform(PathString path, out string platform)
    {
        if (!path.StartsWithSegments(AuthorizationPathPrefix, out var remainingPath))
        {
            platform = string.Empty;
            return false;
        }

        var value = remainingPath.Value;
        if (value is not { Length: > 1 } || value[0] != '/' || value[1] == '/')
        {
            platform = string.Empty;
            return false;
        }

        var nextSeparator = value.IndexOf('/', 1);
        platform = nextSeparator < 0
            ? value[1..]
            : value[1..nextSeparator];

        return !string.IsNullOrWhiteSpace(platform);
    }
}
