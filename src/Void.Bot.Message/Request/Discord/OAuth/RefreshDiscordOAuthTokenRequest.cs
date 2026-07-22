namespace Void.Bot.Message.Request.Discord.OAuth;

public sealed record RefreshDiscordOAuthTokenRequest(string RefreshToken);

public abstract record RefreshDiscordOAuthTokenResponse
{
    public sealed record Succeeded(
        string AccessToken,
        string? RefreshToken,
        DateTimeOffset ExpiresAt) : RefreshDiscordOAuthTokenResponse;

    public sealed record Failed : RefreshDiscordOAuthTokenResponse;
}
