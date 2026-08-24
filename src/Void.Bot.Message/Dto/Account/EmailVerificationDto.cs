namespace Void.Bot.Message.Dto.Account;

public sealed class EmailVerificationDto
{
    public required string Email { get; init; }

    public required Guid ChallengeId { get; init; }

    public required string Code { get; init; }
}

public sealed class EmailVerificationChallengeDto
{
    public Guid ChallengeId { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }
}

public sealed class UserEmailStatusDto
{
    public bool IsBound { get; init; }

    public string? MaskedEmail { get; init; }

    public DateTimeOffset? VerifiedAt { get; init; }
}
