namespace Void.Bot.Message.Enum.ERA;

/// <summary>
/// BUFF 的生命周期类型，由游戏端效果系统执行。
/// </summary>
public enum BuffKind
{
    /// <summary>
    /// 施加时执行一次效果，不进入长期实例池。
    /// </summary>
    Instant = 0,

    /// <summary>
    /// 持续提供属性修正，时间结束或清理时移除。
    /// </summary>
    Persistent = 1,

    /// <summary>
    /// 存续期间按指定间隔执行效果。
    /// </summary>
    Periodic = 2,
}
