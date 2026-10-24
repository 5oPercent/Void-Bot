using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponStatMappingsRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
}

public sealed class GetEraWeaponStatMappingsResponse : EraResponse<PagedListDto<EraWeaponStatMappingDto>>
{
    public long Version { get; set; }
}
