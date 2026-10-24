using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWeaponTemplateRequest : IRequest
{
    public Guid Id { get; set; }
}

public sealed class GetEraWeaponTemplateResponse : EraResponse<EraWeaponTemplateDto>
{
    public long Version { get; set; }
}
