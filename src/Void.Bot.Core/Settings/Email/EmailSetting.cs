using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Email;

public class EmailSetting(IConfiguration configuration) : IConfigurationSetting
{
    private const int DefaultStateTtlSeconds = 600;
    private const int DefaultTokenRefreshIntervalDays = 30;

    public string ClientId { get; } = configuration["Email:ClientId"] ?? string.Empty;
    public string ClientSecret { get; } = configuration["Email:ClientSecret"] ?? string.Empty;
    public string Tenant { get; } = configuration["Email:Tenant"] ?? "consumers";
    public string RedirectUri { get; } = configuration["Email:RedirectUri"] ?? string.Empty;
    public string ExpectedSenderEmail { get; } = configuration["Email:ExpectedSenderEmail"] ?? string.Empty;
    public int StateTtlSeconds { get; } = configuration.GetValue("Email:StateTtlSeconds", DefaultStateTtlSeconds);
    public int TokenRefreshIntervalDays { get; } = configuration.GetValue("Email:TokenRefreshIntervalDays", DefaultTokenRefreshIntervalDays);
}