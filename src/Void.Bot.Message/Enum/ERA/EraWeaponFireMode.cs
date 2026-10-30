namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器框架 Firing 能力的攻击触发方式，具体参数由对应能力配置和模板属性提供。</summary>
public enum EraWeaponFireMode
{
    /// <summary>半自动：每次触发执行一次攻击；BurstCount 必须为 1。</summary>
    SemiAutomatic = 1,
    /// <summary>全自动：持续触发时按频率属性连续攻击；BurstCount 必须为 1。</summary>
    Automatic = 2,
    /// <summary>连发：每次触发执行一轮攻击，次数由 BurstCount 配置，范围为 1～64。</summary>
    Burst = 3,
    /// <summary>蓄力释放：必须同时启用 Charge 能力，完整时长由蓄力时间属性提供；BurstCount 必须为 1。</summary>
    Charged = 4
}
