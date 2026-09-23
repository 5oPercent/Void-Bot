using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Bungie;

public class BungieSetting(IConfiguration configuration) : IConfigurationSetting
{
    public string ClientId { get; set; } = configuration["Bungie:ClientId"] ?? string.Empty;
    public string ClientSecret { get; set; } = configuration["Bungie:ClientSecret"] ?? string.Empty;
    public string ApiKey { get; set; } = configuration["Bungie:ApiKey"] ?? string.Empty;
}