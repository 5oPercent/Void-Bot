namespace Void.Bot.Message.Dto.ERA;

/// <summary>当前配置编辑数据，不参与局内运行模拟。</summary>
public sealed class EraDataItem<T>
{
    /// <summary>稳定 ID。</summary>
    public string Id { get; set; } = "";

    /// <summary>单项版本。</summary>
    public long Version { get; set; }

    /// <summary>返回时的配置修订号。</summary>
    public long Revision { get; set; }

    /// <summary>独立对象定义。</summary>
    public T Definition { get; set; } = default!;
}
