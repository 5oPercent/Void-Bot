using System.Text.Json.Serialization;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;

namespace Void.Bot.Message.Dto.Identity;

public sealed record OAuthStateRecord
{
    public required string State { get; init; }

    public required OAuthStateFlow Flow { get; init; }

    public required AppPlatformEnum Platform { get; init; }

    public required DateTimeOffset IssuedAt { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? InitiatingAccountId { get; init; }
}
