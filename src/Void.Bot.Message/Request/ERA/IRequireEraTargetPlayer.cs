namespace Void.Bot.Message.Request.ERA;

/// <summary>
/// 需要指定目标 ERA 玩家，不要求管理员自身拥有 ERA 玩家档案。
/// </summary>
public interface IRequireEraTargetPlayer
{
    /// <summary>
    /// 需要验证和加载的目标玩家 ID。
    /// </summary>
    Guid PlayerId { get; }
}
