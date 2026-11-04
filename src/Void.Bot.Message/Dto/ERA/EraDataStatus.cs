namespace Void.Bot.Message.Dto.ERA;

/// <summary>当前配置编辑数据，不参与局内运行模拟。</summary>
public sealed class EraDataStatus
{
    /// <summary>当前配置修订号。</summary>
    public long Revision { get; set; }

    /// <summary>种族阵营关系编辑并发令牌。</summary>
    public long SettingsVersion { get; set; }

    /// <summary>种族阵营及默认关系。</summary>
    public EraDataSettings Settings { get; set; } = new();
}
