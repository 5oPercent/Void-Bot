namespace Void.Bot.Message.Dto.ERA;

/// <summary>草稿反向引用分页，不扫描或更改玩家配装／学习记录。</summary>
public sealed class EraSkillDraftReferencePage
{
    /// <summary>引用列表所属草稿修订号。</summary>
    public long DraftRevision { get; set; }
    /// <summary>全部匹配引用数量。</summary>
    public int Total { get; set; }
    /// <summary>当前页引用位置。</summary>
    public List<EraSkillDraftReference> Items { get; set; } = new();
}
