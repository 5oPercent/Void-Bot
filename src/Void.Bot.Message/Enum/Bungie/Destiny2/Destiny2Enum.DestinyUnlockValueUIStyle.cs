using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2解锁值UI样式枚举，定义数值如何在用户界面中显示
/// </summary>
public enum Destiny2DestinyUnlockValueUIStyleEnum
{
    /// <summary>
    /// 自动，通常直接显示数值
    /// </summary>
    [Description("自动")]
    Automatic = 0,

    /// <summary>
    /// 分数，将数值显示为分数形式，适用于有上限的进度显示
    /// </summary>
    [Description("分数")]
    Fraction = 1,

    /// <summary>
    /// 复选框，0表示未选中，其他值表示已选中
    /// </summary>
    [Description("复选框")]
    Checkbox = 2,

    /// <summary>
    /// 百分比，将数值显示为百分比形式
    /// </summary>
    [Description("百分比")]
    Percentage = 3,

    /// <summary>
    /// 日期时间，将数值转换为日期时间格式（Unix时间戳）
    /// </summary>
    [Description("日期时间")]
    DateTime = 4,

    /// <summary>
    /// 浮点分数，显示为0到1之间的浮点值，0为最小值，1为最大值
    /// </summary>
    [Description("浮点分数")]
    FractionFloat = 5,

    /// <summary>
    /// 整数，将数值显示为整数
    /// </summary>
    [Description("整数")]
    Integer = 6,

    /// <summary>
    /// 时间段，将数值（秒）显示为时间段格式
    /// </summary>
    [Description("时间段")]
    TimeDuration = 7,

    /// <summary>
    /// 隐藏，不显示数值，用于内部目的
    /// </summary>
    [Description("隐藏")]
    Hidden = 8,

    /// <summary>
    /// 倍增器，例如"1.5x"
    /// </summary>
    [Description("倍增器")]
    Multiplier = 9,

    /// <summary>
    /// 绿色点，将数值显示为一系列绿色点，例如试炼胜利计分卡
    /// </summary>
    [Description("绿色点")]
    GreenPips = 10,

    /// <summary>
    /// 红色点，将数值显示为一系列红色点，例如试炼失败计分卡
    /// </summary>
    [Description("红色点")]
    RedPips = 11,

    /// <summary>
    /// 显式百分比，直接将数值显示为百分比，例如"51%"，不进行除法
    /// </summary>
    [Description("显式百分比")]
    ExplicitPercentage = 12,

    /// <summary>
    /// 原始浮点数，将数值显示为浮点数，例如"4.52"
    /// </summary>
    [Description("原始浮点")]
    RawFloat = 13,

    /// <summary>
    /// 等级和奖励，同时显示等级和奖励信息
    /// </summary>
    [Description("等级和奖励")]
    LevelAndReward = 14
}