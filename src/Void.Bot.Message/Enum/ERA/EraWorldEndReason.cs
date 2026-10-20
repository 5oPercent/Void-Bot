namespace Void.Bot.Message.Enum.ERA;

/// <summary>服务器确认的世界结束原因，历史重试不覆盖原原因。</summary>
public enum EraWorldEndReason
{
    Abandoned = 1,
    ConstructionFailed = 2,
    LastMemberLeft = 3,
    InactivityTimeout = 4
}
