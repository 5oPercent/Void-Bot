using System.Web;
using Serilog;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Http;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Commands.Bungie;
using Void.Bot.Message.Commands.Bungie.Domain;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Core.Services.Bungie;

public interface IBungieService : IScope
{
    string GenerateAuthenticateUrl(AuthorizationDto dto);

    Task<GenerateBungieTokenResponse> GenerateTokenAsync(GenerateBungieTokenCommand command, CancellationToken cancellationToken);

    Task<RefreshBungieTokenResponse> RefreshTokenAsync(RefreshBungieTokenCommand command, CancellationToken cancellationToken);
}

public class BungieService : IBungieService
{
    private readonly ILogger _logger;
    private readonly BungieSetting _setting;
    private readonly IVoidBotHttpClientFactory _httpClientFactory;
    
    public BungieService(ILogger logger, BungieSetting setting, IVoidBotHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _setting = setting;
        _httpClientFactory = httpClientFactory;
    }
    
    public string GenerateAuthenticateUrl(AuthorizationDto dto)
    {
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["client_id"] = _setting.ClientId;
        parameters["response_type"] = "code";
        parameters["state"] = $"{nameof(dto.Platform)}:{dto.Platform}|{nameof(dto.AccountId)}:{dto.AccountId}";

        return $"{BungieConstants.AuthorizeUrl}?{parameters}";
    }

    public async Task<GenerateBungieTokenResponse> GenerateTokenAsync(GenerateBungieTokenCommand command, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            ["X-API-Key"] = $"{_setting.ApiKey}"
        };

        var dto = new BungieGenerateTokenRequestDto
        {
            ClientId = _setting.ClientId,
            GrantType = "authorization_code",
            Code = command.OAuthCode
        };

        var content = new FormUrlEncodedContent(dto.ToDictionary());

        var response = await _httpClientFactory.PostAsync<BungieGenerateTokenResponseDto>(
            BungieConstants.GenerateTokenUrl, content, cancellationToken, headers: headers);

        return new GenerateBungieTokenResponse
        {
            Data = response.Data
        };
    }
    
    public async Task<RefreshBungieTokenResponse> RefreshTokenAsync(RefreshBungieTokenCommand command, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            ["X-API-Key"] = $"{_setting.ApiKey}"
        };

        var dto = new BungieGenerateTokenRequestDto
        {
            ClientId = _setting.ClientId,
            GrantType = "refresh_token",
            RefreshToken = command.Token
        };
        
        var content = new FormUrlEncodedContent(dto.ToDictionary());
        
        var response = await _httpClientFactory.PostAsync<BungieGenerateTokenResponseDto>(
            BungieConstants.RefreshTokenUrl, content, cancellationToken, headers: headers);

        return new RefreshBungieTokenResponse
        {
            Data = response.Data
        };
    }
}