namespace Void.Bot.Message.Dto.ERA;

/// <summary>技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraSkillDraftSettingsState
{
    /// <summary>设置版本。</summary>
    public long Version { get; set; }

    /// <summary>当前草稿修订号。</summary>
    public long DraftRevision { get; set; }

    /// <summary>当前设置。</summary>
    public EraSkillDraftSettings Settings { get; set; } = new();
}
