using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.OAuth;

public class OAuthSetting(IConfiguration configuration) : IConfigurationSetting
{
    public const int DefaultStateTtlSeconds = 600;
    public const int DefaultExchangeTtlSeconds = 120;
    public const int DefaultRegistrationTtlSeconds = 600;

    public int StateTtlSeconds { get; set; } = configuration.GetValue("OAuth:StateTtlSeconds", DefaultStateTtlSeconds);
    public int ExchangeTtlSeconds { get; set; } = configuration.GetValue("OAuth:ExchangeTtlSeconds", DefaultExchangeTtlSeconds);
    public int RegistrationTtlSeconds { get; set; } = configuration.GetValue("OAuth:RegistrationTtlSeconds", DefaultRegistrationTtlSeconds);
    public string FrontendCallbackUrl { get; set; } = configuration["OAuth:FrontendCallbackUrl"] ?? string.Empty;
}