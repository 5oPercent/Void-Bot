namespace Void.Bot.Message.Dto.ERA;

/// <summary>当前配置编辑数据，不参与局内运行模拟。</summary>
public sealed class EraDataPage<T>
{
    /// <summary>列表所属配置修订号。</summary>
    public long Revision { get; set; }

    /// <summary>筛选后的总记录数。</summary>
    public int Total { get; set; }

    /// <summary>当前分页对象。</summary>
    public List<EraDataItem<T>> Items { get; set; } = new();
}
