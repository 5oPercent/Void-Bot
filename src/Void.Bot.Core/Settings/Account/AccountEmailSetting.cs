using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Account;

public sealed class AccountEmailSetting : IConfigurationSetting
{
    public int CodeTtlMinutes { get; }

    public int ResendIntervalSeconds { get; }

    public int MaxSendsPerHour { get; }

    public int MaxAttempts { get; }

    public AccountEmailSetting(IConfiguration configuration)
    {
        CodeTtlMinutes = configuration.GetValue("AccountEmail:CodeTtlMinutes", 10);
        ResendIntervalSeconds = configuration.GetValue("AccountEmail:ResendIntervalSeconds", 60);
        MaxSendsPerHour = configuration.GetValue("AccountEmail:MaxSendsPerHour", 5);
        MaxAttempts = configuration.GetValue("AccountEmail:MaxAttempts", 5);
    }
}
