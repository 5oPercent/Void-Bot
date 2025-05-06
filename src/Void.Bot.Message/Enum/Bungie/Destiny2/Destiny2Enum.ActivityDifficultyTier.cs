using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运活动难度枚举
/// </summary>
public enum Destiny2ActivityDifficultyTierEnum
{
    /// <summary>
    /// 微不足道
    /// </summary>
    [Description("微不足道")]
    Trivial = 0,
    
    /// <summary>
    /// 简单
    /// </summary>
    [Description("简单")]
    Easy = 1,
    
    /// <summary>
    /// 正常
    /// </summary>
    [Description("正常")]
    Normal = 2,
    
    /// <summary>
    /// 挑战
    /// </summary>
    [Description("挑战")]
    Challenging = 3,
    
    /// <summary>
    /// 困难
    /// </summary>
    [Description("困难")]
    Hard = 4,
    
    /// <summary>
    /// 勇士
    /// </summary>
    [Description("勇士")]
    Brave = 5,
    
    /// <summary>
    /// 几乎不可能
    /// </summary>
    [Description("几乎不可能")]
    AlmostImpossible = 6,
    
    /// <summary>
    /// 不可能
    /// </summary>
    [Description("不可能")]
    Impossible = 7
}