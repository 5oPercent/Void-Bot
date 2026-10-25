using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponSlotConfigurationRequest : IRequest
{
    public Guid Id { get; set; }
}

public sealed class GetEraWeaponSlotConfigurationResponse : EraResponse<EraWeaponSlotConfigurationDto>
{
    public long Version { get; set; }
}
