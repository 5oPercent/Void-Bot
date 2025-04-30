using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2商人进度类型枚举，定义不同类型的商人声望进度系统
/// </summary>
public enum Destiny2VendorProgressionTypeEnum
{
    /// <summary>
    /// 默认进度系统，通过兑换代币获得的传统声望进度
    /// </summary>
    [Description("默认")]
    Default = 0,

    /// <summary>
    /// 仪式进度系统，来自仪式内容的排名，例如竞技场(萨瓦拉)、智谋(流浪者)和第13赛季战场(战术桌)
    /// </summary>
    [Description("仪式活动")]
    Ritual = 1,

    /// <summary>
    /// 无赛季刷新的进度系统，例如30周年纪念活动商人
    /// </summary>
    [Description("无赛季刷新")]
    NoSeasonalRefresh = 2,

    /// <summary>
    /// 未知进度系统
    /// </summary>
    [Description("未知")]
    Unknown = 3
}