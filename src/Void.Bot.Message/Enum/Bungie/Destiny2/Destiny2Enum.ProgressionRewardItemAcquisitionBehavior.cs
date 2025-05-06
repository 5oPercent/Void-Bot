using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 进度奖励物品获取行为枚举，定义玩家获取进度奖励物品的方式
/// </summary>
public enum Destiny2ProgressionRewardItemAcquisitionBehaviorEnum
{
    /// <summary>
    /// 即时获取，奖励物品会自动添加到玩家库存
    /// </summary>
    [Description("即时获取")]
    Instant = 0,

    /// <summary>
    /// 需要玩家领取，玩家必须手动领取奖励物品
    /// </summary>
    [Description("需要玩家领取")]
    PlayerClaimRequired = 1
}