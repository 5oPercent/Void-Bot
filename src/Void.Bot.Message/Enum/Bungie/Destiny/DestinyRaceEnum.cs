using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny;

/// <summary>
/// 命运种族枚举
/// </summary>
public enum DestinyRaceEnum
{
    /// <summary>
    /// 人类
    /// </summary>
    [Description("人类")]
    Human = 0,
    
    /// <summary>
    /// 觉醒者
    /// </summary>
    [Description("觉醒者")]
    Awoken = 1,
    
    /// <summary>
    /// Exo
    /// </summary>
    [Description("Exo")]
    Exo = 2,
    
    /// <summary>
    /// 未知
    /// </summary>
    [Description("未知")]
    Unknown = 3
}