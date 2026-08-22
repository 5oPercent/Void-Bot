using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.OAuth;

public class OAuthSetting : IConfigurationSetting
{
    public const int DefaultStateTtlSeconds = 600;
    public const int DefaultExchangeTtlSeconds = 120;

    public int StateTtlSeconds { get; set; }

    public int ExchangeTtlSeconds { get; set; }

    public string FrontendCallbackUrl { get; set; }

    public OAuthPlatformSetting Discord { get; set; }

    public OAuthSetting(IConfiguration configuration)
    {
        StateTtlSeconds = configuration.GetValue("OAuth:StateTtlSeconds", DefaultStateTtlSeconds);
        ExchangeTtlSeconds = configuration.GetValue("OAuth:ExchangeTtlSeconds", DefaultExchangeTtlSeconds);
        FrontendCallbackUrl = configuration["OAuth:FrontendCallbackUrl"] ?? string.Empty;
        Discord = new OAuthPlatformSetting
        {
            ClientId = configuration["OAuth:Discord:ClientId"] ?? string.Empty,
            ClientSecret = configuration["OAuth:Discord:ClientSecret"] ?? string.Empty
        };
    }
}

public class OAuthPlatformSetting
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}
