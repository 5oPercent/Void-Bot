namespace Void.Bot.Message.Dto.ERA;

/// <summary>技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraSkillDraftPreview
{
    /// <summary>预览冻结的草稿修订号。</summary>
    public long DraftRevision { get; set; }

    /// <summary>预览读取的发布版本。</summary>
    public long PublishedVersion { get; set; }

    /// <summary>阻断发布的配置错误。</summary>
    public List<EraSkillDraftDiagnostic> Errors { get; set; } = new();

    /// <summary>相对于发布配置的增删改摘要。</summary>
    public List<EraSkillDraftReference> Changes { get; set; } = new();

    /// <summary>配置内容是否改变。</summary>
    public bool HasChanges { get; set; }
}
