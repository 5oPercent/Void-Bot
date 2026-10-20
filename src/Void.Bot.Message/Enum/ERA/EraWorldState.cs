namespace Void.Bot.Message.Enum.ERA;

/// <summary>世界实例生命周期；世界身份不随区域推进或房主迁移变化。</summary>
public enum EraWorldState
{
    Building = 1,
    Ready = 2,
    Ending = 3,
    Ended = 4,
    BuildFailed = 5
}
