using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.OAuth;

public class OAuthSetting : IConfigurationSetting
{
    public const int DefaultStateTtlSeconds = 600;
    public const int DefaultExchangeTtlSeconds = 120;

    public int StateTtlSeconds { get; set; }

    public int ExchangeTtlSeconds { get; set; }

    public string FrontendCallbackUrl { get; set; }

    public OAuthSetting(IConfiguration configuration)
    {
        StateTtlSeconds = configuration.GetValue("OAuth:StateTtlSeconds", DefaultStateTtlSeconds);
        ExchangeTtlSeconds = configuration.GetValue("OAuth:ExchangeTtlSeconds", DefaultExchangeTtlSeconds);
        FrontendCallbackUrl = configuration["OAuth:FrontendCallbackUrl"] ?? string.Empty;
    }
}
