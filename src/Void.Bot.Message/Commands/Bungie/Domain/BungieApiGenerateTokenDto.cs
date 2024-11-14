using System.Text.Json.Serialization;

namespace Void.Bot.Message.Commands.Bungie.Domain;

public class BungieApiGenerateTokenRequest
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// 可填 authorization_code / refresh_token
    /// </summary>
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = string.Empty;
    
    /// <summary>
    /// 仅使用 authorization_code 时需要, 传递获取的 Bungie OAuth Code
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 仅使用 refresh_token 时需要, 传递需要刷新的 Bungie Token
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class BungieApiGenerateTokenResponse
{
    /// <summary>
    /// 授权类型
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// 令牌
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Token 有效秒数
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int AccessTokenExpiresIn { get; set; }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌有效秒数
    /// </summary>
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    /// <summary>
    /// Bungie会员Id
    /// </summary>
    [JsonPropertyName("membership_id")]
    public string MembershipId { get; set; } = string.Empty;
    
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
    
    [JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; } = string.Empty;
}