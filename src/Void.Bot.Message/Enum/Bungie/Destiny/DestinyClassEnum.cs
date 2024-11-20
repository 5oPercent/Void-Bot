using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny;

/// <summary>
/// 命运职业枚举
/// </summary>
public enum DestinyClassEnum
{
    /// <summary>
    /// 泰坦
    /// </summary>
    [Description("泰坦")]
    Titan = 0,
    
    /// <summary>
    /// 猎人
    /// </summary>
    [Description("猎人")]
    Hunter = 1,
    
    /// <summary>
    /// 术士
    /// </summary>
    [Description("术士")]
    Warlock = 2,
    
    /// <summary>
    /// 未知
    /// </summary>
    [Description("未知")]
    Unknown = 3
}