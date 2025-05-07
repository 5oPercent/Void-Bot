using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运高亮类型枚举
/// </summary>
public enum Destiny2ActivityGraphNodeHighlightTypeEnum
{
    /// <summary>
    /// 无高亮效果
    /// </summary>
    [Description("无")]
    None = 0,
    
    /// <summary>
    /// 普通高亮效果，标准视觉提示
    /// </summary>
    [Description("普通")]
    Normal = 1,
    
    /// <summary>
    /// 超级高亮效果，用于特别重要的节点
    /// </summary>
    [Description("超级")]
    Hyper = 2,
    
    /// <summary>
    /// 彗星高亮效果，用于主要活动或事件
    /// </summary>
    [Description("彗星")]
    Comet = 3,
    
    /// <summary>
    /// 铁旗高亮效果，源自"铁旗崛起"资料片的特殊高亮样式
    /// </summary>
    [Description("铁旗崛起")]
    RiseOfIron = 4
}