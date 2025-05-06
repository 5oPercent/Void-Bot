using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2破盾类型枚举
/// </summary>
public enum Destiny2BreakerTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 穿盾
    /// </summary>
    [Description("穿盾")]
    ShieldPiercing = 1,

    /// <summary>
    /// 干扰
    /// </summary>
    [Description("干扰")]
    Disruption = 2,

    /// <summary>
    /// 击晕
    /// </summary>
    [Description("击晕")]
    Stagger = 3
}