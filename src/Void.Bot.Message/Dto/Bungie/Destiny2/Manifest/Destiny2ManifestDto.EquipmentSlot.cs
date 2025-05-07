namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 装备插槽定义
/// </summary>
public class Destiny2ManifestEquipmentSlotDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 装备类别哈希
    /// </summary>
    public uint EquipmentCategoryHash { get; set; }
    
    /// <summary>
    /// 背包类型哈希
    /// </summary>
    public uint BucketTypeHash { get; set; }

    /// <summary>
    /// 是否应用自定义染色
    /// </summary>
    public bool ApplyCustomArtDyes { get; set; }

    /// <summary>
    /// 染色通道列表
    /// </summary>
    public List<Destiny2ManifestEquipmentSlotArtDyeChannelEntryDto> ArtDyeChannels { get; set; } = [];
}

/// <summary>
/// 装备插槽染色通道定义
/// </summary>
public class Destiny2ManifestEquipmentSlotArtDyeChannelEntryDto
{
    /// <summary>
    /// 染色通道哈希
    /// </summary>
    public uint ArtDyeChannelHash { get; set; }
}

/// <summary>
/// 装备插槽清单
/// </summary>
public class Destiny2ManifestEquipmentSlotDto : Dictionary<string, Destiny2ManifestEquipmentSlotDefinitionDto>
{
}