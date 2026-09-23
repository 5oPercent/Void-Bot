using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Steam;

public class SteamSetting(IConfiguration configuration) : IConfigurationSetting
{
    public string ApiKey { get; set; } = configuration["Steam:ApiKey"] ?? string.Empty;
}