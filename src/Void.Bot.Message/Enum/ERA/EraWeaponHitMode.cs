namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器命中方式，决定是否需要投射物速度配置。</summary>
public enum EraWeaponHitMode
{
    /// <summary>即时射线命中，不配置飞行投射物速度。</summary>
    Hitscan = 1,
    /// <summary>飞行投射物命中，使用 ProjectileSpeed 配置飞行速度。</summary>
    Projectile = 2
}
