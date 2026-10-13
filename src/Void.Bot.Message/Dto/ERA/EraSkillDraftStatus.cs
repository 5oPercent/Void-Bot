namespace Void.Bot.Message.Dto.ERA;

/// <summary>技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraSkillDraftStatus
{
    /// <summary>当前草稿修订号。</summary>
    public long Revision { get; set; }

    /// <summary>当前游戏可读的发布版本。</summary>
    public long PublishedVersion { get; set; }

    /// <summary>已发布草稿修订号。</summary>
    public long PublishedRevision { get; set; }

    /// <summary>当前内容是否与已发布配置不同。</summary>
    public bool HasUnpublishedChanges { get; set; }

    /// <summary>工作区设置版本。</summary>
    public long SettingsVersion { get; set; }

    /// <summary>通用池和禁用列表。</summary>
    public EraSkillDraftSettings Settings { get; set; } = new();
}
