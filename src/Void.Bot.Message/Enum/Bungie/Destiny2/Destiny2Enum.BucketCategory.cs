using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运库位分类枚举
/// </summary>
public enum Destiny2BucketCategoryEnum
{
    /// <summary>
    /// 不可见库位，通常是系统内部使用的物品槽位
    /// </summary>
    [Description("不可见")]
    Invisible = 0,

    /// <summary>
    /// 普通物品库位，用于存储常规物品
    /// </summary>
    [Description("物品")]
    Item = 1,

    /// <summary>
    /// 货币库位，用于存储游戏中的各种货币
    /// </summary>
    [Description("货币")]
    Currency = 2,

    /// <summary>
    /// 可装备库位，用于存储可以被角色装备的物品
    /// </summary>
    [Description("可装备")]
    Equippable = 3,

    /// <summary>
    /// 被忽略的库位，在UI中不显示的物品槽位
    /// </summary>
    [Description("忽略")]
    Ignored = 4
}