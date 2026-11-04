using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>谐振技能配置管理消息，保存直接生效。</summary>
public sealed class ListEraDataBuffRequest : IRequest
{
    /// <summary>分页跳过量，不小于零。</summary>
    public int Skip { get; set; }
    /// <summary>每页数量，1～100。</summary>
    public int Take { get; set; } = 50;
    /// <summary>按 ID 或名称筛选。</summary>
    public string? Search { get; set; }
}

/// <summary>谐振技能配置操作结果。</summary>
public sealed class ListEraDataBuffResponse : EraResponse<EraDataPage<BuffDefinitionDto>>
{
}
