using System.Web;
using Serilog;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Core.Services.Bungie;

public interface IBungieService : IScope
{
    string GenerateAuthenticateUrl(AuthorizationDto dto);
}

public class BungieService : IBungieService
{
    private readonly ILogger _logger;
    private readonly BungieSetting _setting;
    
    public BungieService(ILogger logger, BungieSetting setting)
    {
        _logger = logger;
        _setting = setting;
    }
    
    public string GenerateAuthenticateUrl(AuthorizationDto dto)
    {
        const string baseUrl = "https://www.bungie.net/zh-chs/OAuth/Authorize";
        
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["client_id"] = _setting.ClientId;
        parameters["response_type"] = "code";
        parameters["state"] = $"{nameof(dto.Platform)}:{dto.Platform}|{nameof(dto.AccountId)}:{dto.AccountId}";

        return $"{baseUrl}?{parameters}";
    }
}