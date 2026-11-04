namespace Void.Bot.Message.Dto.ERA;

/// <summary>当前配置编辑数据，不参与局内运行模拟。</summary>
public sealed class EraDataSettingsState
{
    /// <summary>设置版本。</summary>
    public long Version { get; set; }

    /// <summary>当前配置修订号。</summary>
    public long Revision { get; set; }

    /// <summary>当前设置。</summary>
    public EraDataSettings Settings { get; set; } = new();
}
