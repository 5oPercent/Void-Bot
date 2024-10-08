using System.Web;
using Serilog;
using Void.Bot.Core.Settings;
using Void.Bot.Core.Settings.Steam;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Core.Services.Steam;

public interface ISteamService : IScope
{
    string GenerateAuthenticateUrl(AuthorizationDto dto);
}

public class SteamService : ISteamService
{
    private readonly ILogger _logger;
    private readonly SteamSetting _setting;
    
    public SteamService(ILogger logger, SteamSetting setting)
    {
        _logger = logger;
        _setting = setting;
    }

    public string GenerateAuthenticateUrl(AuthorizationDto dto)
    {
        if (string.IsNullOrEmpty(AppSetting.DomainUrl))
        {
            throw new Exception("DomainUrl is not set");
        }
        
        const string baseUrl = "https://steamcommunity.com/openid/login";
        
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["openid.mode"] = "checkid_setup";
        parameters["openid.ns"] = "http://specs.openid.net/auth/2.0";
        parameters["openid.identity"] = "http://specs.openid.net/auth/2.0/identifier_select";
        parameters["openid.claimed_id"] = "http://specs.openid.net/auth/2.0/identifier_select";
        parameters["openid.return_to"] = AppSetting.DomainUrl + $"/api/Identity/login/steam/{dto.Platform}/{dto.AccountId}";

        return $"{baseUrl}?{parameters}";
    }
}