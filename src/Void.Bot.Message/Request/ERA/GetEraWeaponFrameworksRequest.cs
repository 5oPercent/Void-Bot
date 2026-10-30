using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponFrameworksRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
    public string? PresentationDriverKey { get; set; }
    public bool? IsEnabled { get; set; }
}

public sealed class GetEraWeaponFrameworksResponse : EraResponse<PagedListDto<EraWeaponFrameworkDto>>
{
    public long Version { get; set; }
}
