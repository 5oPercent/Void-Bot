using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 组队查找器选项类型枚举
/// </summary>
public enum Destiny2FireteamFinderOptionValueTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 值列表
    /// </summary>
    [Description("值列表")]
    Values = 1,

    /// <summary>
    /// 玩家数量
    /// </summary>
    [Description("玩家数量")]
    PlayerCount = 2,

    /// <summary>
    /// 组队查找器标签
    /// </summary>
    [Description("组队查找器标签")]
    FireteamFinderLabels = 3,

    /// <summary>
    /// 组队查找器活动图表
    /// </summary>
    [Description("组队查找器活动图表")]
    FireteamFinderActivityGraph = 4
}