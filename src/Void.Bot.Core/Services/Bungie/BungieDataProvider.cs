using System.Web;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Http;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Commands.Bungie.Domain;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieDataProvider : IScope
{
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
            GrantType = "authorization_code",
            Code = oAuthCode
        };
        
        var content = new FormUrlEncodedContent(dto.ToDictionary());

        var response = await _httpClientFactory.PostAsync<BungieApiGenerateTokenResponse>(
            BungieConstants.GenerateTokenUrl, content, cancellationToken, headers: _headers);

        return response.Data;
    }

    public async Task<BungieApiGenerateTokenResponse?> RefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        var dto = new BungieApiGenerateTokenRequest
        {
            ClientId = _setting.ClientId,
            GrantType = "refresh_token",
            RefreshToken = token
        };
        
        var content = new FormUrlEncodedContent(dto.ToDictionary());
        
        var response = await _httpClientFactory.PostAsync<BungieApiGenerateTokenResponse>(
            BungieConstants.RefreshTokenUrl, content, cancellationToken, headers: _headers);

        return response.Data;
    }
}