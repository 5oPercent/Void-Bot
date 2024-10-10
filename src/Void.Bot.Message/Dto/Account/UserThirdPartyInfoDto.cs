namespace Void.Bot.Message.Dto.Account;

public class UserThirdPartyInfoDto
{
    public string Provider { get; set; }
    
    public string ThirdPartyUserId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}