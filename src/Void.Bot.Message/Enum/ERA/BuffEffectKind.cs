namespace Void.Bot.Message.Enum.ERA;

/// <summary>
/// BUFF 执行器支持的效果类型。
/// </summary>
public enum BuffEffectKind
{
    /// <summary>
    /// 持续属性修正，使用 Stat、Operation 与 Amount。
    /// </summary>
    Modifier = 0,

    /// <summary>
    /// 伤害结算，优先消耗目标防护。
    /// </summary>
    Damage = 1,

    /// <summary>
    /// 恢复目标生命值，不超过最大生命值。
    /// </summary>
    Heal = 2,

    /// <summary>
    /// 恢复目标防护值，不超过防护上限。
    /// </summary>
    RestoreProtection = 3,
    /// <summary>限制自主移动、转向、普通攻击和技能。</summary>
    Stun = 4,
    /// <summary>只限制自主移动，不免疫外部位移。</summary>
    Root = 5,
    /// <summary>限制技能释放，不限制普通攻击。</summary>
    Silence = 6,
    /// <summary>新施加眩晕的时长减免，Amount 为 0～1；多来源取最大值。</summary>
    StunResistance = 7,
    RootResistance = 8,
    SilenceResistance = 9,
    /// <summary>阻止新控制，并在免疫存续期间抑制已有同类控制。</summary>
    StunImmunity = 10,
    RootImmunity = 11,
    SilenceImmunity = 12,
}
