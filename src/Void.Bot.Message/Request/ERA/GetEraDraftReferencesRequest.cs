using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>技能草稿验证、导入、发布或历史消息。</summary>
public sealed class GetEraDraftReferencesRequest : IRequest
{
    /// <summary>引用目标类别。</summary>
    public EraSkillDraftKind Kind { get; set; }
    /// <summary>引用目标稳定 ID。</summary>
    public string Id { get; set; } = "";
    /// <summary>分页跳过量，不小于零。</summary>
    public int Skip { get; set; }
    /// <summary>每页数量，1～100。</summary>
    public int Take { get; set; } = 50;
}

/// <summary>操作结果，发布返回完整已发布配置。</summary>
public sealed class GetEraDraftReferencesResponse : EraResponse<EraSkillDraftReferencePage>
{
}
