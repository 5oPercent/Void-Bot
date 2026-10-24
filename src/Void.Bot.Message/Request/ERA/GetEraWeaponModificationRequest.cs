using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponModificationRequest : IRequest
{
    public Guid Id { get; set; }
}

public sealed class GetEraWeaponModificationResponse : EraResponse<EraWeaponModDefinitionDto>
{
    public long Version { get; set; }
}
