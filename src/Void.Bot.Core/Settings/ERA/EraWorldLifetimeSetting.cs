using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.ERA;

/// <summary>世界存活确认策略，独立于 Redis 队伍租约。</summary>
public sealed class EraWorldLifetimeSetting : IConfigurationSetting
{
    public int HeartbeatIntervalSeconds { get; }
    public int InactivityTimeoutSeconds { get; }
    public int MissingConfirmationSeconds { get; }
    public int ScanIntervalSeconds { get; }
    public int BatchSize { get; }

    public EraWorldLifetimeSetting(IConfiguration configuration)
    {
        HeartbeatIntervalSeconds = configuration.GetValue<int?>("Era:World:HeartbeatIntervalSeconds") ?? 15;
        InactivityTimeoutSeconds = configuration.GetValue<int?>("Era:World:InactivityTimeoutSeconds") ?? 60;
        MissingConfirmationSeconds = configuration.GetValue<int?>("Era:World:MissingConfirmationSeconds") ?? 15;
        ScanIntervalSeconds = configuration.GetValue<int?>("Era:World:ScanIntervalSeconds") ?? 10;
        BatchSize = configuration.GetValue<int?>("Era:World:BatchSize") ?? 64;
        if (HeartbeatIntervalSeconds < 5 || HeartbeatIntervalSeconds > 60 ||
            InactivityTimeoutSeconds < HeartbeatIntervalSeconds * 3 || InactivityTimeoutSeconds > 3600 ||
            MissingConfirmationSeconds < ScanIntervalSeconds || MissingConfirmationSeconds > 300 ||
            ScanIntervalSeconds < 1 || ScanIntervalSeconds > 60 || BatchSize < 1 || BatchSize > 256)
        {
            throw new InvalidOperationException("Invalid Era world lifetime configuration.");
        }
    }
}
