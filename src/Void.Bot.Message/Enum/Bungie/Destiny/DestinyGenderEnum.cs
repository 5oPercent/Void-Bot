using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny;

/// <summary>
/// 命运性别枚举
/// </summary>
public enum DestinyGenderEnum
{
    /// <summary>
    /// 男性
    /// </summary>
    [Description("男性")]
    Male = 0,
    
    /// <summary>
    /// 女性
    /// </summary>
    [Description("女性")]
    Female = 1,
    
    /// <summary>
    /// 未知
    /// </summary>
    [Description("未知")]
    Unknown = 2
}