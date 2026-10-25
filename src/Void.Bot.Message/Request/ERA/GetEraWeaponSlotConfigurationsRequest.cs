using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponSlotConfigurationsRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
}

public sealed class GetEraWeaponSlotConfigurationsResponse : EraResponse<PagedListDto<EraWeaponSlotConfigurationDto>>
{
    public long Version { get; set; }
}
