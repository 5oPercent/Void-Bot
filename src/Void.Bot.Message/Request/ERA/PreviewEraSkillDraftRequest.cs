using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>技能草稿验证、导入、发布或历史消息。</summary>
public sealed class PreviewEraSkillDraftRequest : IRequest
{
}

/// <summary>操作结果，发布返回完整已发布配置。</summary>
public sealed class PreviewEraSkillDraftResponse : EraResponse<EraSkillDraftPreview>
{
}
