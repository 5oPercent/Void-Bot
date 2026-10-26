using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraArmorSlotConfigurationsRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
}

public sealed class GetEraArmorSlotConfigurationsResponse : EraResponse<PagedListDto<EraArmorSlotConfigurationDto>>
{
    public long Version { get; set; }
}
