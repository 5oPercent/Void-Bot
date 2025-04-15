using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运无法加入火力队伍原因枚举
/// </summary>
public enum Destiny2JoinClosedReasonsEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,
    
    /// <summary>
    /// 匹配中
    /// </summary>
    [Description("匹配中")]
    InMatchmaking = 1,
    
    /// <summary>
    /// 玩家处于加载中
    /// </summary>
    [Description("加载中")]
    Loading = 2,
    
    /// <summary>
    /// 单人模式
    /// </summary>
    [Description("单人模式")]
    SoloMode = 3,
    
    /// <summary>
    /// 服务器阻止加入
    /// </summary>
    [Description("服务器阻止加入")]
    InternalReasons = 4,
    
    /// <summary>
    /// 活动阻止, 一般发生于活动到尾声或者已经结束
    /// </summary>
    [Description("活动阻止")]
    DisallowedByGameState = 5,
    
    /// <summary>
    /// 离线
    /// </summary>
    [Description("离线")]
    Offline = 32768
}