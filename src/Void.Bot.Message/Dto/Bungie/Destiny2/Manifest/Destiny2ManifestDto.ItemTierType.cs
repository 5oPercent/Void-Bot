namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 物品等级类型定义，描述物品的稀有度和品质级别
/// </summary>
public class Destiny2ManifestItemTierTypeDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 定义此等级物品的注入机制
    /// </summary>
    public Destiny2ManifestItemTierTypeInfusionBlockDto InfusionProcess { get; set; } = new();
}

/// <summary>
/// 物品等级类型注入，定义物品注入和升级的参数
/// </summary>
public class Destiny2ManifestItemTierTypeInfusionBlockDto
{
    /// <summary>
    /// 基础品质转移比率，决定注入时转移的基础能量百分比
    /// </summary>
    public float BaseQualityTransferRatio { get; set; }
    
    /// <summary>
    /// 最小品质增量，注入时保证的最小能量提升值
    /// </summary>
    public int MinimumQualityIncrement { get; set; }
}

/// <summary>
/// 物品等级类型清单，存储所有物品等级类型定义的字典集合
/// </summary>
public class Destiny2ManifestItemTierTypeDto : Dictionary<string, Destiny2ManifestItemTierTypeDefinitionDto>
{
}