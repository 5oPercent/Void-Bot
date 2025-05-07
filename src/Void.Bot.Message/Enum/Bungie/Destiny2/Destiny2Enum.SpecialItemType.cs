using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2特殊物品类型枚举
/// </summary>
public enum Destiny2SpecialItemTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,
    
    /// <summary>
    /// 特殊货币
    /// </summary>
    [Description("特殊货币")]
    SpecialCurrency = 1,
    
    /// <summary>
    /// 护甲
    /// </summary>
    [Description("护甲")]
    Armor = 8,
    
    /// <summary>
    /// 武器
    /// </summary>
    [Description("武器")]
    Weapon = 9,
    
    /// <summary>
    /// 英格拉姆
    /// </summary>
    [Description("英格拉姆")]
    Engram = 23,
    
    /// <summary>
    /// 消耗品
    /// </summary>
    [Description("消耗品")]
    Consumable = 24,
    
    /// <summary>
    /// 交换材料
    /// </summary>
    [Description("交换材料")]
    ExchangeMaterial = 25,
    
    /// <summary>
    /// 任务奖励
    /// </summary>
    [Description("任务奖励")]
    MissionReward = 27,
    
    /// <summary>
    /// 货币
    /// </summary>
    [Description("货币")]
    Currency = 29
}