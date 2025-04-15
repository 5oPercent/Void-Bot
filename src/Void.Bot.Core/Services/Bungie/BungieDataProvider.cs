using System.Web;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Http;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Dto.Bungie;
using Void.Bot.Message.Dto.HttpClient;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieDataProvider : IScope
{
    HttpClient GetBungieHttpClient(string token = "", TimeSpan? timeout = null, bool beginScope = false);
    
    string GenerateAuthenticateUrl(AppPlatformEnum platform, string accountId);
    
    Task<BungieApiGenerateTokenResponse?> GenerateTokenAsync(string oAuthCode, CancellationToken cancellationToken);
    
    Task<BungieApiGenerateTokenResponse?> RefreshTokenAsync(string token, CancellationToken cancellationToken);
}

public partial class BungieDataProvider : IBungieDataProvider
{
    private readonly BungieSetting _setting;
    private readonly IVoidBotHttpClientFactory _httpClientFactory;
    private readonly Dictionary<string, string> _headers;
    
    public BungieDataProvider(BungieSetting setting, IVoidBotHttpClientFactory httpClientFactory)
    {
        _setting = setting;
        _httpClientFactory = httpClientFactory;
        
        _headers = new Dictionary<string, string>
        {
            ["X-API-Key"] = $"{_setting.ApiKey}"
        };
    }

    public HttpClient GetBungieHttpClient(string token = "", TimeSpan? timeout = null, bool beginScope = false)
    {
        TokenModel? tokenModel = null;
        
        if (!string.IsNullOrEmpty(token))
        {
            tokenModel = new TokenModel { Parameter = token };
        }

        var client = _httpClientFactory.CreateClient(tokenModel, _headers, timeout, beginScope);
        
        return client;
    }

    public string GenerateAuthenticateUrl(AppPlatformEnum platform, string accountId)
    {
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["client_id"] = _setting.ClientId;
        parameters["response_type"] = "code";
        parameters["state"] = $"{nameof(platform)}:{platform}|{nameof(accountId)}:{accountId}";
        
        return $"{BungieConstants.AuthorizeUrl}?{parameters}";
    }

    public async Task<BungieApiGenerateTokenResponse?> GenerateTokenAsync(string oAuthCode, CancellationToken cancellationToken)
    {
        var dto = new BungieApiGenerateTokenRequest
        {
            ClientId = _setting.ClientId,
            ClientSecret = _setting.ClientSecret,
            GrantType = "authorization_code",
            Code = oAuthCode
        };
        
        var content = new FormUrlEncodedContent(dto.ToDictionary());

        var response = await GetBungieHttpClient().PostAsync<BungieApiGenerateTokenResponse>(
            BungieConstants.GenerateTokenUrl, content, cancellationToken);

        return response.Data;
    }

    public async Task<BungieApiGenerateTokenResponse?> RefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        var dto = new BungieApiGenerateTokenRequest
        {
            ClientId = _setting.ClientId,
            ClientSecret = _setting.ClientSecret,
            GrantType = "refresh_token",
            RefreshToken = token
        };
        
        var content = new FormUrlEncodedContent(dto.ToDictionary());
        
        var response = await GetBungieHttpClient().PostAsync<BungieApiGenerateTokenResponse>(
            BungieConstants.RefreshTokenUrl, content, cancellationToken);

        return response.Data;
    }
}