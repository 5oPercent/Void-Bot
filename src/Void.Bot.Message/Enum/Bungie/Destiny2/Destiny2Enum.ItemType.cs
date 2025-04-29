using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2物品类型枚举
/// </summary>
public enum Destiny2ItemTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 货币
    /// </summary>
    [Description("货币")]
    Currency = 1,

    /// <summary>
    /// 护甲
    /// </summary>
    [Description("护甲")]
    Armor = 2,

    /// <summary>
    /// 武器
    /// </summary>
    [Description("武器")]
    Weapon = 3,

    /// <summary>
    /// 消息
    /// </summary>
    [Description("消息")]
    Message = 7,

    /// <summary>
    /// 英格拉姆
    /// </summary>
    [Description("英格拉姆")]
    Engram = 8,

    /// <summary>
    /// 消耗品
    /// </summary>
    [Description("消耗品")]
    Consumable = 9,

    /// <summary>
    /// 交换材料
    /// </summary>
    [Description("交换材料")]
    ExchangeMaterial = 10,

    /// <summary>
    /// 任务奖励
    /// </summary>
    [Description("任务奖励")]
    MissionReward = 11,

    /// <summary>
    /// 任务步骤
    /// </summary>
    [Description("任务步骤")]
    QuestStep = 12,

    /// <summary>
    /// 任务步骤完成
    /// </summary>
    [Description("任务步骤完成")]
    QuestStepComplete = 13,

    /// <summary>
    /// 徽章
    /// </summary>
    [Description("徽章")]
    Emblem = 14,

    /// <summary>
    /// 任务
    /// </summary>
    [Description("任务")]
    Quest = 15,

    /// <summary>
    /// 子职业
    /// </summary>
    [Description("子职业")]
    Subclass = 16,

    /// <summary>
    /// 公会旗帜
    /// </summary>
    [Description("公会旗帜")]
    ClanBanner = 17,

    /// <summary>
    /// 光环
    /// </summary>
    [Description("光环")]
    Aura = 18,

    /// <summary>
    /// 模组
    /// </summary>
    [Description("模组")]
    Mod = 19,

    /// <summary>
    /// 占位符
    /// </summary>
    [Description("占位符")]
    Dummy = 20,

    /// <summary>
    /// 飞船
    /// </summary>
    [Description("飞船")]
    Ship = 21,

    /// <summary>
    /// 载具
    /// </summary>
    [Description("载具")]
    Vehicle = 22,

    /// <summary>
    /// 表情
    /// </summary>
    [Description("表情")]
    Emote = 23,

    /// <summary>
    /// 幽灵
    /// </summary>
    [Description("幽灵")]
    Ghost = 24,

    /// <summary>
    /// 包裹
    /// </summary>
    [Description("包裹")]
    Package = 25,

    /// <summary>
    /// 悬赏
    /// </summary>
    [Description("悬赏")]
    Bounty = 26,

    /// <summary>
    /// 包装
    /// </summary>
    [Description("包装")]
    Wrapper = 27,

    /// <summary>
    /// 赛季神器
    /// </summary>
    [Description("赛季神器")]
    SeasonalArtifact = 28,

    /// <summary>
    /// 终结技
    /// </summary>
    [Description("终结技")]
    Finisher = 29,

    /// <summary>
    /// 图纸
    /// </summary>
    [Description("图纸")]
    Pattern = 30
}