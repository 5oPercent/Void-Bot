namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器框架 Reload 能力的装填方式；基础时长由能力配置提供，实际时长受装填倍率影响。</summary>
public enum EraWeaponReloadMode
{
    /// <summary>整匣装填：按一次完整装填流程补充当前装载资源，容量仍为有限上限。</summary>
    Magazine = 1,
    /// <summary>逐发装填：按单发步骤逐次补充当前装载资源，容量仍为有限上限。</summary>
    PerRound = 2
}
