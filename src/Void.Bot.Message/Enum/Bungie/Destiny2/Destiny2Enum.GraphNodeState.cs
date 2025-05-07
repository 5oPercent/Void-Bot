using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2活动图节点状态枚举，定义节点在活动图中的显示和交互状态
/// </summary>
public enum Destiny2GraphNodeStateEnum
{
    /// <summary>
    /// 隐藏状态，节点对玩家完全不可见
    /// </summary>
    [Description("隐藏")]
    Hidden = 0,
    
    /// <summary>
    /// 可见状态，节点对玩家可见但可能无法交互
    /// </summary>
    [Description("可见")]
    Visible = 1,
    
    /// <summary>
    /// 预告状态，节点显示为预告形式，通常表示即将开放的内容
    /// </summary>
    [Description("预告")]
    Teaser = 2,
    
    /// <summary>
    /// 未完成状态，节点可见且可交互，但相关活动未完成
    /// </summary>
    [Description("未完成")]
    Incomplete = 3,
    
    /// <summary>
    /// 已完成状态，节点相关的活动或目标已被玩家完成
    /// </summary>
    [Description("已完成")]
    Completed = 4
}