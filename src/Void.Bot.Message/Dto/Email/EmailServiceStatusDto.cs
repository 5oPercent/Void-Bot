namespace Void.Bot.Message.Dto.Email;

public sealed class EmailServiceStatusDto
{
    public bool IsAuthorized { get; init; }

    public string SenderEmail { get; init; } = string.Empty;

    public string Status { get; init; } = "NotAuthorized";

    public DateTimeOffset? AccessTokenExpiresAt { get; init; }

    public DateTimeOffset? RefreshTokenLastUsedAt { get; init; }

    public DateTimeOffset? NextRefreshAt { get; init; }

    public string? LastError { get; init; }

    public DateTimeOffset? LastErrorAt { get; init; }
}
