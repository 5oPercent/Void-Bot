namespace Void.Bot.Message.Dto.Account;

public class UserAccountInfoDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public string Suffix { get; set; } = string.Empty;
    
    public string DisplayName => $"{Name}#{Suffix}";
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    
    public List<UserThirdPartyInfoDto> ThirdParties { get; set; } = new();
}