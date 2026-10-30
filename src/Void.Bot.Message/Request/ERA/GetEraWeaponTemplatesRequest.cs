using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponTemplatesRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
    /// <summary>按武器框架标识筛选；不填时不限制框架，可与名称或标识搜索组合。</summary>
    public Guid? FrameworkId { get; set; }
}

public sealed class GetEraWeaponTemplatesResponse : EraResponse<PagedListDto<EraWeaponTemplateDto>>
{
    public long Version { get; set; }
}
