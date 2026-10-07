namespace Void.Bot.Message.Enum.ERA;

/// <summary>仅表示官方端点协调状态，不代表 ENet 握手或游戏身份认证完成。</summary>
public enum EraPunchState
{
    NotPrepared = 0,
    WaitingForRegistration = 1,
    WaitingForPeer = 2,
    ReadyForPunch = 3,
    TicketExpired = 4
}
