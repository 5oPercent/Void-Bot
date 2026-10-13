using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>技能草稿管理消息，保存不直接发布。</summary>
public sealed class ListEraDraftSystemRequest : IRequest
{
    /// <summary>分页跳过量，不小于零。</summary>
    public int Skip { get; set; }
    /// <summary>每页数量，1～100。</summary>
    public int Take { get; set; } = 50;
    /// <summary>按 ID 或名称筛选。</summary>
    public string? Search { get; set; }
    /// <summary>是否包含删除标记。</summary>
    public bool IncludeDeleted { get; set; }
}

/// <summary>技能草稿操作结果。</summary>
public sealed class ListEraDraftSystemResponse : EraResponse<EraSkillDraftPage<AbilitySystemDefinition>>
{
}
