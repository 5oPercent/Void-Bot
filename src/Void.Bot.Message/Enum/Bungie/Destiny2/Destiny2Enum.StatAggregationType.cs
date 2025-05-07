using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2统计值聚合类型枚举
/// </summary>
public enum Destiny2StatAggregationTypeEnum
{
    /// <summary>
    /// 角色平均值
    /// </summary>
    [Description("角色平均值")]
    CharacterAverage = 0,

    /// <summary>
    /// 角色
    /// </summary>
    [Description("角色")]
    Character = 1,

    /// <summary>
    /// 物品
    /// </summary>
    [Description("物品")]
    Item = 2
}