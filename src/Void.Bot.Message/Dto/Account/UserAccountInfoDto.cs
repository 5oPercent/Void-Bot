namespace Void.Bot.Message.Dto.Account;

public class UserAccountInfoDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string Suffix { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    
    public List<UserThirdPartyInfoDto> ThirdParties { get; set; } = new();
}