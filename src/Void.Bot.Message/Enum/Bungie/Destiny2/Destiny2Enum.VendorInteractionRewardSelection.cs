using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2商人交互奖励选择枚举，定义与商人交互时玩家可以选择的奖励数量
/// </summary>
public enum Destiny2VendorInteractionRewardSelectionEnum
{
    /// <summary>
    /// 无法选择奖励
    /// </summary>
    [Description("无")]
    None = 0,
    
    /// <summary>
    /// 可以选择一个奖励
    /// </summary>
    [Description("单选")]
    One = 1,
    
    /// <summary>
    /// 可以选择所有奖励
    /// </summary>
    [Description("全选")]
    All = 2
}