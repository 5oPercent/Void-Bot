namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器弹药类型，由武器模板配置；战术与重型分别计算补给分配。</summary>
public enum EraWeaponAmmoType
{
    /// <summary>无限备弹：储备容量为 -1，不参与弹药补给分配；持有此类武器不影响个人弹药掉落。</summary>
    Infinite = 0,
    /// <summary>战术弹药：有限备弹，可由战术或通用弹药包恢复。</summary>
    Tactical = 1,
    /// <summary>重型弹药：有限备弹，可由重型或通用弹药包恢复。</summary>
    Heavy = 2
}
