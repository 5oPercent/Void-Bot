namespace Void.Bot.Message.Dto.Discord.OAuth;

public sealed record DiscordOAuthIdentity(
    string UserId,
    string DisplayName,
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset ExpiresAt);
