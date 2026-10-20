namespace Void.Bot.Message.Enum.ERA;

/// <summary>后台超时检查结果，不作为客户端权限或资产结果。</summary>
public enum EraWorldTimeoutStatus
{
    Deferred = 1,
    Alive = 2,
    Missing = 3,
    Ended = 4,
    Unavailable = 5,
    NotFound = 6
}
