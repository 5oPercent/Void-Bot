using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraArmorSlotConfigurationRequest : IRequest
{
    public Guid Id { get; set; }
}

public sealed class GetEraArmorSlotConfigurationResponse : EraResponse<EraArmorSlotConfigurationDto>
{
    public long Version { get; set; }
}
