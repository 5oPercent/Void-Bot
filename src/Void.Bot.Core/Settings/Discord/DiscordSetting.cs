using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Discord;

public class DiscordSetting(IConfiguration configuration) : IConfigurationSetting
{
    public bool EnableProxy { get; set; } = configuration.GetValue<bool>("Discord:EnableProxy");
    public bool AutoStart { get; set; } = configuration.GetValue<bool>("Discord:AutoStart");
    public string Token { get; set; } = configuration["Discord:Token"] ?? string.Empty;
    public string ClientId { get; set; } = configuration["Discord:ClientId"] ?? string.Empty;
    public string ClientSecret { get; set; } = configuration["Discord:ClientSecret"] ?? string.Empty;
}