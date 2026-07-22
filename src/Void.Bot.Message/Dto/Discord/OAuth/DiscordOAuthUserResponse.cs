using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.Discord.OAuth;

public sealed class DiscordOAuthUserResponse
{
    public string? Id { get; init; }

    [JsonPropertyName("global_name")]
    public string? GlobalName { get; init; }

    public string? Username { get; init; }
}
