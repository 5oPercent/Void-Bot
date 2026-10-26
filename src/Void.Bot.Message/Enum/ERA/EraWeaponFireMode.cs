namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器模板的开火模式，配合行为参数使用。</summary>
public enum EraWeaponFireMode
{
    /// <summary>半自动开火。</summary>
    SemiAutomatic = 1,
    /// <summary>全自动开火。</summary>
    Automatic = 2,
    /// <summary>点射开火，单轮发数由 BurstCount 配置。</summary>
    Burst = 3,
    /// <summary>蓄力开火，蓄力时长由 ChargeTime 配置。</summary>
    Charged = 4
}
