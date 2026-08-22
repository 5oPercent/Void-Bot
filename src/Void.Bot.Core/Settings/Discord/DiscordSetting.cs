using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Discord;

public class DiscordSetting : IConfigurationSetting
{
    public bool EnableProxy { get; set; }
    
    public bool AutoStart { get; set; }
    
    public string Token { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
    
    public DiscordSetting(IConfiguration configuration)
    {
        EnableProxy = configuration.GetValue<bool>("Discord:EnableProxy");
        AutoStart = configuration.GetValue<bool>("Discord:AutoStart");
        Token = configuration["Discord:Token"] ?? string.Empty;
        ClientId = configuration["Discord:ClientId"] ?? string.Empty;
        ClientSecret = configuration["Discord:ClientSecret"] ?? string.Empty;
    }
}
