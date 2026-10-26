using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraArmorModificationRequest : IRequest
{
    public Guid Id { get; set; }
}

public sealed class GetEraArmorModificationResponse : EraResponse<EraArmorModDefinitionDto>
{
    public long Version { get; set; }
}
