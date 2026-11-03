using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>敌人配置草稿管理消息，保存不直接发布。</summary>
public sealed class GetEraDraftEnemyRequest : IRequest
{
    /// <summary>路由写入的稳定对象 ID。</summary>
    public string Id { get; set; } = "";
}

/// <summary>敌人配置草稿操作结果。</summary>
public sealed class GetEraDraftEnemyResponse : EraResponse<EraResonanceDraftItem<EnemyDefinition>>
{
}
