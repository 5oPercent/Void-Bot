namespace Void.Bot.Message.Commands.Bungie.Domain;

public class BungieGenerateTokenRequestDto
{
    public string Client_Id { get; set; }
    
    /// <summary>
    /// 可填 authorization_code / refresh_token
    /// </summary>
    public string Grant_Type { get; set; }
    
    /// <summary>
    /// 仅使用 authorization_code 时需要, 传递获取的 Bungie OAuth Code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 仅使用 refresh_token 时需要, 传递需要刷新的 Bungie Token
    /// </summary>
    public string Refresh_Token { get; set; }
}

public class BungieGenerateTokenResponseDto
{
    public string Access_Token { get; set; }
    
    public string Token_Type { get; set; }

    /// <summary>
    /// Token 有效秒数
    /// </summary>
    public int Expires_In { get; set; }
    
    /// <summary>
    /// Bungie会员Id
    /// </summary>
    public string Membership_Id { get; set; }
    
    public string Error { get; set; }
    
    public string Error_Description { get; set; }
}