using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.Discord.OAuth;

public sealed class DiscordOAuthTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("expires_in")]
    public long? ExpiresIn { get; init; }

    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }
}
