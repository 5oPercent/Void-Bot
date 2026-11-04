using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>谐振技能配置管理消息，保存直接生效。</summary>
public sealed class GetEraDataSettingsRequest : IRequest
{
}

/// <summary>谐振技能配置操作结果。</summary>
public sealed class GetEraDataSettingsResponse : EraResponse<EraDataSettingsState>
{
}
