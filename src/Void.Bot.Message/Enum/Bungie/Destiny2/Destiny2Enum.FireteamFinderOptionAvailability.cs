using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 组队查找器选项可用性枚举
/// </summary>
public enum Destiny2FireteamFinderOptionAvailabilityEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 创建清单构建器
    /// </summary>
    [Description("创建清单构建器")]
    CreateListingBuilder = 1,

    /// <summary>
    /// 搜索清单构建器
    /// </summary>
    [Description("搜索清单构建器")]
    SearchListingBuilder = 2,

    /// <summary>
    /// 清单查看器
    /// </summary>
    [Description("清单查看器")]
    ListingViewer = 4,

    /// <summary>
    /// 大厅查看器
    /// </summary>
    [Description("大厅查看器")]
    LobbyViewer = 8
}