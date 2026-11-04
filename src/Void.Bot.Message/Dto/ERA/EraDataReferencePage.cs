namespace Void.Bot.Message.Dto.ERA;

/// <summary>配置反向引用分页，不扫描或更改玩家配装／学习记录。</summary>
public sealed class EraDataReferencePage
{
    /// <summary>引用列表所属配置修订号。</summary>
    public long Revision { get; set; }
    /// <summary>全部匹配引用数量。</summary>
    public int Total { get; set; }
    /// <summary>当前页引用位置。</summary>
    public List<EraDataReference> Items { get; set; } = new();
}
