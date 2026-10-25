using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponModificationsRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
    public Guid? SlotTypeId { get; set; }
}

public sealed class GetEraWeaponModificationsResponse : EraResponse<PagedListDto<EraWeaponModDefinitionDto>>
{
    public long Version { get; set; }
}
