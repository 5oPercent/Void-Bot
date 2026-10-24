using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponStatMappingRequest : IRequest
{
    public Guid Id { get; set; }
}

public sealed class GetEraWeaponStatMappingResponse : EraResponse<EraWeaponStatMappingDto>
{
    public long Version { get; set; }
}
