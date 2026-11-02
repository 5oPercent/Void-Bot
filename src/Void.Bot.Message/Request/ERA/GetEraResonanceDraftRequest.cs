using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>谐振技能草稿管理消息，保存不直接发布。</summary>
public sealed class GetEraResonanceDraftRequest : IRequest
{
}

/// <summary>谐振技能草稿操作结果。</summary>
public sealed class GetEraResonanceDraftResponse : EraResponse<EraResonanceDraftStatus>
{
}
