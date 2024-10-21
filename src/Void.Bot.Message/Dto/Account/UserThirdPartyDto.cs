namespace Void.Bot.Message.Dto.Account;

public class UserThirdPartyDto
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public string Provider { get; set; }
    
    public string ThirdPartyUserId { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
    
    public string ExtraData { get; set; } = string.Empty;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
}