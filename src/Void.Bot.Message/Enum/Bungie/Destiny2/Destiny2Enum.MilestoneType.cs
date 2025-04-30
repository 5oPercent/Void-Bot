using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2里程碑类型枚举，定义不同的里程碑重置和出现方式
/// </summary>
public enum Destiny2MilestoneTypeEnum
{
    /// <summary>
    /// 未知类型里程碑
    /// </summary>
    [Description("未知")]
    Unknown = 0,

    /// <summary>
    /// 教程类里程碑，特别面向教导玩家新机制和游戏模式的一次性里程碑
    /// </summary>
    [Description("教程")]
    Tutorial = 1,

    /// <summary>
    /// 一次性里程碑，完成一次后永远不能重复的里程碑
    /// </summary>
    [Description("一次性")]
    OneTime = 2,

    /// <summary>
    /// 每周里程碑，每周重置的里程碑。它们不必都在同一天或时间重置，但需要每周重置才能归类为此类型
    /// </summary>
    [Description("每周")]
    Weekly = 3,

    /// <summary>
    /// 每日里程碑，每天重置的里程碑
    /// </summary>
    [Description("每日")]
    Daily = 4,

    /// <summary>
    /// 特殊里程碑，表示事件不是按日/周节奏出现，但确实会出现多次。例如，命运1中的铁旗活动或黎明节是"特殊"事件的例子
    /// </summary>
    [Description("特殊")]
    Special = 5
}