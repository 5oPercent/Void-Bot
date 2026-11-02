namespace Void.Bot.Message.Dto.ERA;

/// <summary>谐振技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraResonanceDraftPage<T>
{
    /// <summary>列表所属草稿修订号。</summary>
    public long DraftRevision { get; set; }

    /// <summary>筛选后的总记录数。</summary>
    public int Total { get; set; }

    /// <summary>当前分页对象。</summary>
    public List<EraResonanceDraftItem<T>> Items { get; set; } = new();
}
