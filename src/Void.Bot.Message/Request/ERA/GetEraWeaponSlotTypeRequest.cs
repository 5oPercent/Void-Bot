using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponSlotTypeRequest : IRequest
{
    public Guid Id { get; set; }
}

public sealed class GetEraWeaponSlotTypeResponse : EraResponse<EraWeaponSlotTypeDto>
{
    public long Version { get; set; }
}
