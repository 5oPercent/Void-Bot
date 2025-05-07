using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 库存大小类型定义
/// </summary>
public class Destiny2ManifestInventoryBucketDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 库位作用域
    /// </summary>
    public Destiny2BucketScopeEnum Scope { get; set; }

    /// <summary>
    /// 库位分类
    /// </summary>
    public Destiny2BucketCategoryEnum Category { get; set; }
    
    /// <summary>
    /// 槽位顺序，用于排序显示
    /// </summary>
    public int BucketOrder { get; set; }

    /// <summary>
    /// 物品数量上限
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// 位置标识符，表示库存槽位在UI中的位置
    /// </summary>
    public int Location { get; set; }

    /// <summary>
    /// 是否可以将物品传送至该槽位
    /// </summary>
    public bool HasTransferDestination { get; set; }

    /// <summary>
    /// 是否启用该槽位
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// 是否使用先进先出(FIFO)顺序管理物品
    /// </summary>
    public bool Fifo { get; set; }
}

/// <summary>
/// 库存大小类型清单
/// </summary>
public class Destiny2ManifestInventoryBucketDto : Dictionary<string, Destiny2ManifestInventoryBucketDefinitionDto>
{
}