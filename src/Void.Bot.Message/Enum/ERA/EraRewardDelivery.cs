namespace Void.Bot.Message.Enum.ERA;

/// <summary>由来源配置决定，来源类型不限制发放方式。</summary>
public enum EraRewardDelivery
{
    /// <summary>直接发放到目标角色背包。</summary>
    Inventory = 1,
    /// <summary>以正式地面装备掉落发放，由拾取流程进入背包。</summary>
    Ground = 2
}
