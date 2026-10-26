namespace Void.Bot.Message.Enum.ERA;

/// <summary>世界实例生命周期；世界身份不随区域推进或房主迁移变化。</summary>
public enum EraWorldState
{
    /// <summary>世界正在构建。</summary>
    Building = 1,
    /// <summary>构建完成，世界已就绪。</summary>
    Ready = 2,
    /// <summary>世界正在结束。</summary>
    Ending = 3,
    /// <summary>世界已结束，保留世界身份和历史记录。</summary>
    Ended = 4,
    /// <summary>世界构建失败。</summary>
    BuildFailed = 5
}
