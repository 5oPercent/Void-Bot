using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponFrameworksRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
    /// <summary>按框架分类精确筛选；为空时不限制分类，最多 64 字符。</summary>
    public string? Category { get; set; }
    public string? PresentationDriverKey { get; set; }
    public bool? IsEnabled { get; set; }
}

public sealed class GetEraWeaponFrameworksResponse : EraResponse<PagedListDto<EraWeaponFrameworkDto>>
{
    public long Version { get; set; }
}
