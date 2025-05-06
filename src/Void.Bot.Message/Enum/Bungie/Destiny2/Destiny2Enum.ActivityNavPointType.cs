using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2活动导航点类型枚举，定义不同类型的游戏内导航标记点
/// </summary>
public enum Destiny2ActivityNavPointTypeEnum
{
    /// <summary>
    /// 不活跃的导航点
    /// </summary>
    [Description("不活跃")]
    Inactive = 0,
    
    /// <summary>
    /// 主要目标点
    /// </summary>
    [Description("主要目标")]
    PrimaryObjective = 1,
    
    /// <summary>
    /// 次要目标点
    /// </summary>
    [Description("次要目标")]
    SecondaryObjective = 2,
    
    /// <summary>
    /// 旅行目标点
    /// </summary>
    [Description("旅行目标")]
    TravelObjective = 3,
    
    /// <summary>
    /// 公共事件目标点
    /// </summary>
    [Description("公共事件")]
    PublicEventObjective = 4,
    
    /// <summary>
    /// 弹药缓存点
    /// </summary>
    [Description("弹药缓存")]
    AmmoCache = 5,
    
    /// <summary>
    /// 点类型标记
    /// </summary>
    [Description("点类型标记")]
    PointTypeFlag = 6,
    
    /// <summary>
    /// 占领点
    /// </summary>
    [Description("占领点")]
    CapturePoint = 7,
    
    /// <summary>
    /// 防御遭遇战
    /// </summary>
    [Description("防御遭遇战")]
    DefensiveEncounter = 8,
    
    /// <summary>
    /// 幽灵互动点
    /// </summary>
    [Description("幽灵互动")]
    GhostInteraction = 9,
    
    /// <summary>
    /// 击杀AI目标
    /// </summary>
    [Description("击杀AI")]
    KillAi = 10,
    
    /// <summary>
    /// 任务物品
    /// </summary>
    [Description("任务物品")]
    QuestItem = 11,
    
    /// <summary>
    /// 巡逻任务点
    /// </summary>
    [Description("巡逻任务")]
    PatrolMission = 12,
    
    /// <summary>
    /// 来袭点
    /// </summary>
    [Description("来袭")]
    Incoming = 13,
    
    /// <summary>
    /// 竞技场目标
    /// </summary>
    [Description("竞技场目标")]
    ArenaObjective = 14,
    
    /// <summary>
    /// 自动化提示
    /// </summary>
    [Description("自动化提示")]
    AutomationHint = 15,
    
    /// <summary>
    /// 跟踪任务
    /// </summary>
    [Description("跟踪任务")]
    TrackedQuest = 16
}