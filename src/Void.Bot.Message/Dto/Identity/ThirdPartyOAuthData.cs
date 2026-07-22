using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.Identity;

public abstract class ThirdPartyOAuthData
{
    public required string Platform { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RedirectUrl { get; init; }
}
