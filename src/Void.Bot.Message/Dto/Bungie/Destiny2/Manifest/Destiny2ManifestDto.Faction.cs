namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 派系定义
/// </summary>
public class Destiny2ManifestFactionDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 表示与该派系在经验和等级方面的关系哈希
    /// </summary>
    public uint ProgressionHash { get; set; }

    /// <summary>
    /// 派系代币物品哈希值及其相应的进度值
    /// </summary>
    public Dictionary<uint, uint> TokenValues { get; set; } = new();

    /// <summary>
    /// 奖励物品哈希
    /// </summary>
    public uint RewardItemHash { get; set; }

    /// <summary>
    /// 奖励供应商哈希
    /// </summary>
    public uint RewardVendorHash { get; set; }
    
    /// <summary>
    /// 供应商
    /// </summary>
    public List<Destiny2ManifestFactionVendorDto> Vendors { get; set; } = [];
}

/// <summary>
/// 命运2清单派系供应商
/// </summary>
public class Destiny2ManifestFactionVendorDto
{
    /// <summary>
    /// 供应商哈希
    /// </summary>
    public uint VendorHash { get; set; }
    
    /// <summary>
    /// 目的地哈希
    /// </summary>
    public uint DestinationHash { get; set; }
    
    /// <summary>
    /// 背景图片
    /// </summary>
    public string BackgroundImagePath { get; set; }
}

/// <summary>
/// 派系清单
/// </summary>
public class Destiny2ManifestFactionDto : Dictionary<string, Destiny2ManifestFactionDefinitionDto>
{
}