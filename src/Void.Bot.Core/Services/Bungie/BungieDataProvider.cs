using System.Web;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Destiny2;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Http;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Dto.Bungie;
using Void.Bot.Message.Dto.HttpClient;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieDataProvider : IScope
{
    HttpClient GetBungieHttpClient(string token = "", TimeSpan? timeout = null, bool beginScope = false);
    
    string GenerateOAuthAuthorizationUrl(string state);
    
    Task<BungieApiGenerateTokenResponse?> GenerateTokenAsync(string oAuthCode, CancellationToken cancellationToken);
    
    Task<BungieApiGenerateTokenResponse?> RefreshTokenAsync(string token, CancellationToken cancellationToken);
    
    Task<BungieApiNetUserResponse?> GetNetUserAsync(string id, CancellationToken cancellationToken);
    
    Task<BungieApiMembershipsByIdResponse?> GetMembershipsByIdAsync(
        BungieMembershipTypeEnum membershipType, string membershipId, CancellationToken cancellationToken);
}

public partial class BungieDataProvider : IBungieDataProvider
{
    private readonly BungieSetting _setting;
    private readonly IVoidBotHttpClientFactory _httpClientFactory;
    private readonly Dictionary<string, string> _headers;
    private readonly IRepository<Destiny2SeasonHistory> _seasonHistoryRepository;
    private readonly IRepository<Destiny2Pgcr> _pgcrRepository;
    private readonly IRepository<Destiny2PgcrPlayer> _pgcrPlayerRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public BungieDataProvider(
        BungieSetting setting,
        IVoidBotHttpClientFactory httpClientFactory,
        IRepository<Destiny2SeasonHistory> seasonHistoryRepository,
        IRepository<Destiny2Pgcr> pgcrRepository,
        IRepository<Destiny2PgcrPlayer> pgcrPlayerRepository,
        IUnitOfWork unitOfWork)
    {
        _setting = setting;
        _httpClientFactory = httpClientFactory;
        
        _headers = new Dictionary<string, string>
        {
            ["X-API-Key"] = $"{_setting.ApiKey}"
        };
        
        _seasonHistoryRepository = seasonHistoryRepository;
        _pgcrRepository = pgcrRepository;
        _pgcrPlayerRepository = pgcrPlayerRepository;
        _unitOfWork = unitOfWork;
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

    public string GenerateOAuthAuthorizationUrl(string state)
    {
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["client_id"] = _setting.ClientId;
        parameters["response_type"] = "code";
        parameters["state"] = state;

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

    public async Task<BungieApiNetUserResponse?> GetNetUserAsync(string id, CancellationToken cancellationToken)
    {
        var url = BungieConstants.NetUserUrl
            .Replace("{id}", id, StringComparison.OrdinalIgnoreCase);
        
        var response = await GetBungieHttpClient().GetAsync<BungieApiNetUserResponse>(url, cancellationToken);
        
        return response.Data;
    }

    public async Task<BungieApiMembershipsByIdResponse?> GetMembershipsByIdAsync(
        BungieMembershipTypeEnum membershipType, string membershipId, CancellationToken cancellationToken)
    {
        var dto = new BungieApiMembershipsByIdRequest
        {
            MembershipId = membershipId,
            MembershipType = membershipType
        };
        
        var url = BungieConstants.MembershipsUrl
            .Replace("{membershipId}", membershipId, StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase);
        
        var response = await GetBungieHttpClient().GetAsync<BungieApiMembershipsByIdResponse>(url, cancellationToken);
        
        return response.Data;
    }
}
