using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 组队查找器选项类型枚举
/// </summary>
public enum Destiny2FireteamFinderOptionTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 仅限应用程序
    /// </summary>
    [Description("仅限应用程序")]
    ApplicationOnly = 1,

    /// <summary>
    /// 仅在线
    /// </summary>
    [Description("仅在线")]
    OnlineOnly = 2,

    /// <summary>
    /// 玩家数量
    /// </summary>
    [Description("玩家数量")]
    PlayerCount = 3,

    /// <summary>
    /// 标题
    /// </summary>
    [Description("标题")]
    Title = 4,

    /// <summary>
    /// 标签
    /// </summary>
    [Description("标签")]
    Tags = 5,

    /// <summary>
    /// 查找器活动图
    /// </summary>
    [Description("查找器活动图")]
    FinderActivityGraph = 6,

    /// <summary>
    /// 需要麦克风
    /// </summary>
    [Description("需要麦克风")]
    MicrophoneRequired = 7
}