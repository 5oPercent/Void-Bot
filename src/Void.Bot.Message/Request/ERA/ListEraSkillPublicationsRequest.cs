using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>技能草稿验证、导入、发布或历史消息。</summary>
public sealed class ListEraSkillPublicationsRequest : IRequest
{
    /// <summary>分页跳过量，不小于零。</summary>
    public int Skip { get; set; }
    /// <summary>每页数量，1～100。</summary>
    public int Take { get; set; } = 50;
}

/// <summary>操作结果，发布返回完整已发布配置。</summary>
public sealed class ListEraSkillPublicationsResponse : EraResponse<EraSkillPublicationPage>
{
}
