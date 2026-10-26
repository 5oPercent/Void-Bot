namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器属性标识；评分属性结合模板引用的属性映射换算为实际效果。</summary>
public enum EraWeaponStat
{
    /// <summary>单发基础伤害。</summary>
    Damage = 1,
    /// <summary>射击频率，单位次／秒；10 表示每秒 10 发。</summary>
    Frequency = 2,
    /// <summary>弹匣装载上限，单位发，不是当前剩余弹量。</summary>
    LoadedCapacity = 3,
    /// <summary>本武器独立储备弹药上限，单位发。</summary>
    ReserveCapacity = 4,
    /// <summary>装填速度系数；1 为基础速度，1.2 表示速度提高 20%。</summary>
    ReloadSpeed = 5,
    /// <summary>射程，单位米。</summary>
    Range = 6,
    /// <summary>精准度评分，范围 0～100，由属性映射换算为散布角。</summary>
    Accuracy = 7,
    /// <summary>稳定性评分，范围 0～100，由属性映射换算为后坐幅度。</summary>
    Stability = 8,
    /// <summary>操控性评分，范围 0～100，由属性映射换算为举枪和瞄准耗时。</summary>
    Handling = 9,
    /// <summary>投射物速度，单位米／秒，仅适用于飞行投射物。</summary>
    ProjectileSpeed = 10,
    /// <summary>完整蓄力时间，单位秒，仅适用于蓄力开火。</summary>
    ChargeTime = 11
}
