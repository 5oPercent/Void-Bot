using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2插槽类别样式枚举，定义不同类型插槽在界面中的显示和行为方式
/// </summary>
public enum Destiny2SocketCategoryStyleEnum
{
    /// <summary>
    /// 未知类型，默认插槽样式
    /// </summary>
    [Description("未知")]
    Unknown = 0,

    /// <summary>
    /// 可重用，可多次替换的插槽类型
    /// </summary>
    [Description("可重用")]
    Reusable = 1,

    /// <summary>
    /// 消耗型，使用后会被消耗的插槽类型
    /// </summary>
    [Description("消耗型")]
    Consumable = 2,

    /// <summary>
    /// 可解锁，需要特定条件才能解锁使用的插槽类型
    /// </summary>
    [Description("可解锁")]
    Unlockable = 3,

    /// <summary>
    /// 内在属性，装备固有属性的插槽类型
    /// </summary>
    [Description("内在属性")]
    Intrinsic = 4,

    /// <summary>
    /// 能量计量，显示为能量条的插槽类型
    /// </summary>
    [Description("能量计量")]
    EnergyMeter = 5,

    /// <summary>
    /// 大型特性，显示为大图标的重要特性插槽类型
    /// </summary>
    [Description("大型特性")]
    LargePerk = 6,

    /// <summary>
    /// 能力，角色能力相关的插槽类型
    /// </summary>
    [Description("能力")]
    Abilities = 7,

    /// <summary>
    /// 超能力，角色超能力相关的插槽类型
    /// </summary>
    [Description("超能力")]
    Supers = 8
}