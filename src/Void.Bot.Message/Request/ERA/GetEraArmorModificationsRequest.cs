using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraArmorModificationsRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
    public Guid? SlotTypeId { get; set; }
}

public sealed class GetEraArmorModificationsResponse : EraResponse<PagedListDto<EraArmorModDefinitionDto>>
{
    public long Version { get; set; }
}
