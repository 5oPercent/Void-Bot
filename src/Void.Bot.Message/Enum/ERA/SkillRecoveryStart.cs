namespace Void.Bot.Message.Enum.ERA;

/// <summary>
/// 使用次数冷却恢复的起点，扣除次数与开始恢复是独立时机。
/// </summary>
public enum SkillRecoveryStart
{
    /// <summary>
    /// 成功释放后开始恢复。
    /// </summary>
    Released = 0,

    /// <summary>
    /// 载体结束后开始恢复。
    /// </summary>
    CarrierEnded = 1,

    /// <summary>
    /// 载体回收后开始恢复；未回收就被销毁时也开始恢复。
    /// </summary>
    CarrierRecalled = 2,
}
