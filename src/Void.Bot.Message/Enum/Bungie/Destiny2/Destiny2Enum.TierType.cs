using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运层级枚举
/// </summary>
public enum Destiny2TierTypeEnum
{
    /// <summary>
    /// 未知
    /// </summary>
    [Description("未知")]
    Unknown = 0,
    
    /// <summary>
    /// 货币
    /// </summary>
    [Description("货币")]
    Currency = 1,
    
    
    /// <summary>
    /// 基础
    /// </summary>
    [Description("基础")]
    Basic = 2,
    
    /// <summary>
    /// 普通
    /// </summary>
    [Description("普通")]
    Common = 3,
    
    /// <summary>
    /// 稀有
    /// </summary>
    [Description("稀有")]
    Rare = 4,
    
    /// <summary>
    /// 优质
    /// </summary>
    [Description("优质")]
    Superior = 5,
    
    /// <summary>
    /// 异域
    /// </summary>
    [Description("异域")]
    Exotic = 6
}