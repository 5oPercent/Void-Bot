using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Account;

public sealed class TwoFactorSetting : IConfigurationSetting
{
    public string Title { get; }

    public int AllowedTimeStepDrift { get; }

    public int SetupExpireMinutes { get; }

    public int RecoveryCodeCount { get; }

    public TwoFactorSetting(IConfiguration configuration)
    {
        Title = configuration["TwoFactor:Title"]?.Trim() is { Length: > 0 } title
            ? title
            : "Void Bot";
        AllowedTimeStepDrift = Math.Clamp(
            configuration.GetValue("TwoFactor:AllowedTimeStepDrift", 1), 0, 2);
        SetupExpireMinutes = Math.Clamp(
            configuration.GetValue("TwoFactor:SetupExpireMinutes", 10), 1, 60);
        RecoveryCodeCount = Math.Clamp(
            configuration.GetValue("TwoFactor:RecoveryCodeCount", 8), 1, 20);
    }
}
