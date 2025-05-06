namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 目的地定义
/// </summary>
public class Destiny2ManifestDestinationDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 地区哈希值，用于关联此目的地所属的地区
    /// </summary>
    public uint PlaceHash { get; set; }
    
    /// <summary>
    /// 默认自由漫游活动哈希，指向进入此目的地时默认加载的活动
    /// </summary>
    public uint DefaultFreeroamActivityHash { get; set; }

    /// <summary>
    /// 活动图表条目列表，包含此目的地关联的所有活动图
    /// </summary>
    public List<Destiny2ManifestActivityGraphListEntryDto> ActivityGraphEntries { get; set; } = [];
    
    /// <summary>
    /// 气泡区域列表，描述目的地中的可探索区域
    /// </summary>
    public List<Destiny2ManifestBubbleDefinitionDto> Bubbles { get; set; } = [];
}


/// <summary>
/// 目的地清单
/// </summary>
public class Destiny2ManifestDestinationDto : Dictionary<string, Destiny2ManifestDestinationDefinitionDto>
{
}