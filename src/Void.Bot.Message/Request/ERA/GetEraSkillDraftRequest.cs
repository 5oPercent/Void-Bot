using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>技能草稿管理消息，保存不直接发布。</summary>
public sealed class GetEraSkillDraftRequest : IRequest
{
}

/// <summary>技能草稿操作结果。</summary>
public sealed class GetEraSkillDraftResponse : EraResponse<EraSkillDraftStatus>
{
}
