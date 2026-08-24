using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Email;

public class EmailSetting : IConfigurationSetting
{
    public const int DefaultStateTtlSeconds = 600;
    public const int DefaultTokenRefreshIntervalDays = 30;

    public string ClientId { get; }

    public string ClientSecret { get; }

    public string Tenant { get; }

    public string RedirectUri { get; }

    public string ExpectedSenderEmail { get; }

    public int StateTtlSeconds { get; }

    public int TokenRefreshIntervalDays { get; }

    public EmailSetting(IConfiguration configuration)
    {
        ClientId = configuration["Email:ClientId"] ?? string.Empty;
        ClientSecret = configuration["Email:ClientSecret"] ?? string.Empty;
        Tenant = configuration["Email:Tenant"] ?? "consumers";
        RedirectUri = configuration["Email:RedirectUri"] ?? string.Empty;
        ExpectedSenderEmail = configuration["Email:ExpectedSenderEmail"] ?? string.Empty;
        StateTtlSeconds = configuration.GetValue("Email:StateTtlSeconds", DefaultStateTtlSeconds);
        TokenRefreshIntervalDays = configuration.GetValue(
            "Email:TokenRefreshIntervalDays", DefaultTokenRefreshIntervalDays);
    }
}
