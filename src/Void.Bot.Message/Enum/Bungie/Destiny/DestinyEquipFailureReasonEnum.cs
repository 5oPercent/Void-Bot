using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny;

/// <summary>
/// 命运无法装备失败原因枚举
/// </summary>
public enum DestinyEquipFailureReasonEnum
{
    /// <summary>
    /// 允许装备
    /// </summary>
    [Description("允许装备")]
    None = 0,
    
    /// <summary>
    /// 不可装备
    /// </summary>
    [Description("不可装备")]
    ItemUnequippable = 1,
    
    /// <summary>
    /// 异域装备限制
    /// </summary>
    [Description("异域装备限制")]
    ItemUniqueEquipRestricted = 2,
    
    /// <summary>
    /// 装备解锁检查失败
    /// </summary>
    [Description("条件不符合")]
    ItemFailedUnlockCheck = 4,
    
    /// <summary>
    /// 装备等级检查失败
    /// </summary>
    [Description("等级不符合")]
    ItemFailedLevelCheck = 8,
    
    /// <summary>
    /// 商品包装(不明)
    /// </summary>
    [Description("ItemWrapped")]
    ItemWrapped = 16,
    
    /// <summary>
    /// 未加载
    /// </summary>
    [Description("装备未加载")]
    ItemNotLoaded = 32,
    
    /// <summary>
    /// 阻止列表
    /// </summary>
    [Description("处于阻止列表")]
    ItemEquipBlocklisted = 64,
    
    /// <summary>
    /// 装备需求未满足
    /// </summary>
    [Description("装备需求未满足")]
    ItemLoadoutRequirementNotMet = 128,
}