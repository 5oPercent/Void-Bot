using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>谐振技能配置管理消息，保存直接生效。</summary>
public sealed class GetEraDataResonanceRequest : IRequest
{
    /// <summary>路由写入的稳定对象 ID。</summary>
    public string Id { get; set; } = "";
}

/// <summary>谐振技能配置操作结果。</summary>
public sealed class GetEraDataResonanceResponse : EraResponse<EraDataItem<ResonanceDefinition>>
{
}
