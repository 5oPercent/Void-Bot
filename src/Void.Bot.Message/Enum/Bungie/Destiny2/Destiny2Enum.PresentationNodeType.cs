using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2展示节点类型枚举，定义了展示节点的不同类型
/// </summary>
public enum Destiny2PresentationNodeTypeEnum
{
    /// <summary>
    /// 默认类型
    /// </summary>
    [Description("默认")]
    Default = 0,
    
    /// <summary>
    /// 类别节点，用于组织和分类其他节点
    /// </summary>
    [Description("类别")]
    Category = 1,
    
    /// <summary>
    /// 收藏品节点，显示可收集的物品
    /// </summary>
    [Description("收藏品")]
    Collectibles = 2,
    
    /// <summary>
    /// 记录节点，显示成就和进度记录
    /// </summary>
    [Description("记录")]
    Records = 3,
    
    /// <summary>
    /// 指标节点，显示玩家统计数据和指标
    /// </summary>
    [Description("指标")]
    Metric = 4,
    
    /// <summary>
    /// 可制作节点，显示可制作的物品
    /// </summary>
    [Description("可制作")]
    Craftable = 5
}