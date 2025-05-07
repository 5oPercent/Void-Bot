using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2统计类别枚举
/// </summary>
public enum Destiny2StatCategoryEnum
{
    /// <summary>
    /// 游戏玩法
    /// </summary>
    [Description("游戏玩法")]
    Gameplay = 0,

    /// <summary>
    /// 武器
    /// </summary>
    [Description("武器")]
    Weapon = 1,

    /// <summary>
    /// 防御
    /// </summary>
    [Description("防御")]
    Defense = 2,

    /// <summary>
    /// 主要
    /// </summary>
    [Description("主要")]
    Primary = 3
}