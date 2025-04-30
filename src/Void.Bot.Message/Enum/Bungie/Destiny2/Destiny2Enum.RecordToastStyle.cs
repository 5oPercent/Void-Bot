using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 记录提示样式枚举，定义成就记录完成时的通知提示样式
/// </summary>
public enum Destiny2RecordToastStyleEnum
{
    /// <summary>
    /// 无样式，不显示提示通知
    /// </summary>
    [Description("无样式")]
    None = 0,

    /// <summary>
    /// 记录完成，显示标准记录完成提示
    /// </summary>
    [Description("记录完成")]
    Record = 1,

    /// <summary>
    /// 传说解锁，显示游戏传说故事解锁提示
    /// </summary>
    [Description("传说解锁")]
    Lore = 2,

    /// <summary>
    /// 徽章获得，显示徽章收集完成提示
    /// </summary>
    [Description("徽章获得")]
    Badge = 3,

    /// <summary>
    /// 元记录，显示综合成就完成提示
    /// </summary>
    [Description("元记录")]
    MetaRecord = 4,

    /// <summary>
    /// 勋章完成，显示勋章获得提示
    /// </summary>
    [Description("勋章完成")]
    MedalComplete = 5,

    /// <summary>
    /// 赛季挑战完成，显示赛季挑战完成提示
    /// </summary>
    [Description("赛季挑战完成")]
    SeasonChallengeComplete = 6,

    /// <summary>
    /// 镀金称号完成，显示称号镀金完成提示
    /// </summary>
    [Description("镀金称号完成")]
    GildedTitleComplete = 7,

    /// <summary>
    /// 制作配方解锁，显示新武器/装备制作配方解锁提示
    /// </summary>
    [Description("制作配方解锁")]
    CraftingRecipeUnlocked = 8,

    /// <summary>
    /// 守护者等级提示，显示守护者等级相关信息提示
    /// </summary>
    [Description("守护者等级提示")]
    ToastGuardianRankDetails = 9,

    /// <summary>
    /// 探路者仪式目标完成，显示探路者仪式活动目标完成提示
    /// </summary>
    [Description("探路者仪式目标完成")]
    PathfinderObjectiveCompleteRituals = 10,

    /// <summary>
    /// 探路者裂痕目标完成，显示探路者裂痕活动目标完成提示
    /// </summary>
    [Description("探路者裂痕目标完成")]
    PathfinderObjectiveCompleteSchism = 11,

    /// <summary>
    /// 探路者PVP目标完成，显示探路者PVP活动目标完成提示
    /// </summary>
    [Description("探路者PVP目标完成")]
    PathfinderObjectiveCompletePvp = 12,

    /// <summary>
    /// 探路者打击目标完成，显示探路者打击活动目标完成提示
    /// </summary>
    [Description("探路者打击目标完成")]
    PathfinderObjectiveCompleteStrikes = 13,

    /// <summary>
    /// 探路者智谋目标完成，显示探路者智谋活动目标完成提示
    /// </summary>
    [Description("探路者智谋目标完成")]
    PathfinderObjectiveCompleteGambit = 14
}