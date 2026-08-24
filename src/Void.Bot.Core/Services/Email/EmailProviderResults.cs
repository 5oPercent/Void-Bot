namespace Void.Bot.Core.Services.Email;

internal sealed class EmailTokenResult
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required string IdToken { get; init; }

    public required DateTimeOffset AccessTokenExpiresAt { get; init; }
}

internal sealed class EmailIdentityResult
{
    public required string AccountId { get; init; }

    public required string Email { get; init; }
}
