namespace Void.Bot.Message.Enum.ERA;

/// <summary>效果的存续类型；周期类型允许全部效果条目。</summary>
public enum BuffKind
{
    /// <summary>执行一次结算，不保留实例，不支持需要撤销的属性修正。</summary>
    Instant = 0,

    /// <summary>保留实例；正数间隔重复结算，-1 不重复。属性修正随实例存续。</summary>
    Periodic = 2,
}
