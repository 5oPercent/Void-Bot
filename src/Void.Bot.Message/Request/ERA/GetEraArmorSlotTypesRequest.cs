using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraArmorSlotTypesRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
}

public sealed class GetEraArmorSlotTypesResponse : EraResponse<PagedListDto<EraArmorSlotTypeDto>>
{
    public long Version { get; set; }
}
