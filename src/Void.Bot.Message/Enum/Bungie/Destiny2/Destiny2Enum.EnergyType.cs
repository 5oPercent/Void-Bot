using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运能量类型枚举
/// </summary>
public enum Destiny2EnergyTypeEnum
{
    /// <summary>
    /// 任意
    /// </summary>
    [Description("任意")]
    Any = 0,

    /// <summary>
    /// 电弧
    /// </summary>
    [Description("电弧")]
    Arc = 1,

    /// <summary>
    /// 烈日
    /// </summary>
    [Description("烈日")]
    Thermal = 2,

    /// <summary>
    /// 虚空
    /// </summary>
    [Description("虚空")]
    Void = 3,

    /// <summary>
    /// 幽灵
    /// </summary>
    [Description("幽灵")]
    Ghost = 4,

    /// <summary>
    /// 子职业
    /// </summary>
    [Description("子职业")]
    Subclass = 5,

    /// <summary>
    /// 冰影
    /// </summary>
    [Description("冰影")]
    Stasis = 6
}