namespace Void.Bot.Message.Enum.ERA;

/// <summary>正式地面装备掉落的生命周期状态。</summary>
public enum EraEquipmentDropStatus
{
    /// <summary>可领取，仍需通过所有权、分享期限和世界状态校验。</summary>
    Available = 1,
    /// <summary>已被领取，不可再次拾取。</summary>
    PickedUp = 2,
    /// <summary>掉落已失效，不可再领取。</summary>
    Invalidated = 3,
    /// <summary>被有拾取权限的玩家主动销毁。</summary>
    Destroyed = 4
}
