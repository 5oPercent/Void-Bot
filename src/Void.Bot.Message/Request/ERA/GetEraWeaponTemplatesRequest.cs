using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponTemplatesRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
}

public sealed class GetEraWeaponTemplatesResponse : EraResponse<PagedListDto<EraWeaponTemplateDto>>
{
    public long Version { get; set; }
}
