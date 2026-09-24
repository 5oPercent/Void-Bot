namespace Void.Bot.Message.Dto.Account;

public class UserAccountSecurityInfoDto : UserAccountInfoDto
{
    public string? Email { get; set; }

    public DateTimeOffset? EmailVerifiedAt { get; set; }
}
