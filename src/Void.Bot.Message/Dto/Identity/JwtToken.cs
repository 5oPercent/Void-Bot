namespace Void.Bot.Message.Dto.Identity;

public class JwtToken
{
    public string AccessToken { get; set; }
    
    public string RefreshToken { get; set; }
}