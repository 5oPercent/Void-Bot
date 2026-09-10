namespace Void.Bot.Message.Dto.Account;

public sealed class TwoFactorRecoveryCodesDto
{
    public IReadOnlyList<string> RecoveryCodes { get; init; } = [];
}
