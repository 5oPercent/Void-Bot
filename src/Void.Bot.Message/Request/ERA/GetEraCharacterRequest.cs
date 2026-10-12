using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraCharacterRequest : IRequest, IRequireEraPlayer
{
    public Guid CharacterId { get; set; }
}

public class GetEraCharacterResponse : EraResponse<EraCharacterDto>
{
}
