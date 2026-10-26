using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraArmorSlotTypeRequest : IRequest
{
    public Guid Id { get; set; }
}

public sealed class GetEraArmorSlotTypeResponse : EraResponse<EraArmorSlotTypeDto>
{
    public long Version { get; set; }
}
