using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponSlotTypesRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
}

public sealed class GetEraWeaponSlotTypesResponse : EraResponse<PagedListDto<EraWeaponSlotTypeDto>>
{
    public long Version { get; set; }
}
