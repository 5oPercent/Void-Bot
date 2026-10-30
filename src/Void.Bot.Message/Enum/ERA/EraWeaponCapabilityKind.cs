namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器框架可组合的能力类型；决定配置对象和必需的补充属性，不保存具体武器的属性值。</summary>
public enum EraWeaponCapabilityKind
{
    /// <summary>瞄准与散布能力；要求精度属性，散布角度由模板精度属性的两个效果端点换算。</summary>
    Aim = 1,
    /// <summary>后坐能力；要求稳定属性，后坐角度由模板稳定属性的两个效果端点换算。</summary>
    Recoil = 2,
    /// <summary>装填能力；要求装填倍率属性，装填方式和基础时长由 Reload 配置提供。</summary>
    Reload = 3,
    /// <summary>蓄力能力；要求蓄力时间属性，由 Charge 配置决定是否允许提前释放。</summary>
    Charge = 4,
    /// <summary>飞行投射物能力；要求投射物速度属性，最大飞行距离由 Projectile 配置提供。</summary>
    Projectile = 5,
    /// <summary>近战 Hit 命中能力；动作与碰撞体决定命中窗口及范围，穿透属性限制有效目标数量。</summary>
    MeleeHit = 6,
    /// <summary>攻击触发能力；由 Firing 配置半自动、全自动、连发或蓄力释放及连发次数。</summary>
    Firing = 7,
    /// <summary>射线命中能力；由 Hitscan 配置最大检测距离，不要求投射物速度属性。</summary>
    Hitscan = 8
}
