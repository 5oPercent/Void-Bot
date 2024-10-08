using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Bungie;

public class BungieSetting : IConfigurationSetting
{
    public string ClientId { get; set; }
    public string ApiKey { get; set; }
    public string OAuthRedirectUri { get; set; }
    
    public BungieSetting(IConfiguration configuration)
    {
        ClientId = configuration["Bungie:ClientId"] ?? string.Empty;;
        ApiKey = configuration["Bungie:ApiKey"] ?? string.Empty;
        OAuthRedirectUri = configuration["Bungie:OAuthRedirectUri"] ?? string.Empty;
    }
}