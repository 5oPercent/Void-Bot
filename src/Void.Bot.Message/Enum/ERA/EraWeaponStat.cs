namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器属性标识；六项公共属性始终存在，补充属性按框架能力启用，评分结合模板属性端点换算为实际效果。</summary>
public enum EraWeaponStat
{
    /// <summary>伤害：单次命中的基础伤害，有限数且范围为 0～1000000。</summary>
    Damage = 1,
    /// <summary>频率：每秒攻击次数，必须为有限正数且不超过 1000000。</summary>
    Frequency = 2,
    /// <summary>容量：当前装载上限，范围为 1～1000000 的整数，始终有限且不允许 -1。</summary>
    LoadedCapacity = 3,
    /// <summary>储备：备用弹药上限，范围为 -1～1000000 的整数；-1 表示无限储备，当前装载量仍有限。</summary>
    ReserveCapacity = 4,
    /// <summary>装填：装填速度倍率，必须为有限正数且不超过 1000000；1 为基础速度，实际时长为基础秒数除以最终倍率。</summary>
    ReloadSpeed = 5,
    /// <summary>穿透：首次命中后可继续命中的目标数，范围为 -1～1000000 的整数；0 最多命中一个目标，N 最多 N+1 个，-1 无数量上限，不代表穿墙。</summary>
    Penetration = 6,
    /// <summary>精度：范围为 0～100 的评分，通过模板属性的两个效果端点换算为散布角度；要求 Aim 能力。</summary>
    Accuracy = 7,
    /// <summary>稳定：范围为 0～100 的评分，通过模板属性的两个效果端点换算为后坐角度；要求 Recoil 能力。</summary>
    Stability = 8,
    /// <summary>操控：范围为 0～100 的评分，通过模板属性的两个效果端点仅决定掏出当前武器的时间，单位秒。</summary>
    Handling = 9,
    /// <summary>投射物速度：单位米／秒，必须为有限正数且不超过 1000000；要求 Projectile 能力。</summary>
    ProjectileSpeed = 10,
    /// <summary>蓄力时间：完成一次完整蓄力所需的秒数，必须为有限正数且不超过 3600；要求 Charge 能力。</summary>
    ChargeTime = 11
}
