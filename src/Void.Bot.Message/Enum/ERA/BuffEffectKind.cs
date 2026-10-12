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
}
