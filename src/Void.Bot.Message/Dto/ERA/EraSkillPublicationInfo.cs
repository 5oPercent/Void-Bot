namespace Void.Bot.Message.Dto.ERA;

/// <summary>技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraSkillPublicationInfo
{
    /// <summary>发布版本。</summary>
    public long Version { get; set; }

    /// <summary>来源草稿修订号。</summary>
    public long DraftRevision { get; set; }

    /// <summary>内容摘要。</summary>
    public string ContentHash { get; set; } = "";

    /// <summary>发布账号。</summary>
    public Guid? PublishedBy { get; set; }

    /// <summary>发布时间。</summary>
    public DateTimeOffset PublishedAt { get; set; }
}
