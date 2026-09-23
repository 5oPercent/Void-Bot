using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Account;

public sealed class AccountEmailSetting(IConfiguration configuration) : IConfigurationSetting
{
    public int CodeTtlMinutes { get; } = configuration.GetValue("AccountEmail:CodeTtlMinutes", 10);
    public int ResendIntervalSeconds { get; } = configuration.GetValue("AccountEmail:ResendIntervalSeconds", 60);
    public int MaxSendsPerHour { get; } = configuration.GetValue("AccountEmail:MaxSendsPerHour", 5);
    public int MaxAttempts { get; } = configuration.GetValue("AccountEmail:MaxAttempts", 5);
    public int PasswordResetLinkTtlMinutes { get; } = configuration.GetValue("AccountEmail:PasswordResetLinkTtlMinutes", 15);
    public string PasswordResetFrontendUrl { get; } = configuration["AccountEmail:PasswordResetFrontendUrl"] ?? string.Empty;
}