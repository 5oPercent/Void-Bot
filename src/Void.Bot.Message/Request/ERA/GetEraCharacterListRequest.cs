using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraCharacterListRequest : IRequest
{
}

public class GetEraCharacterListResponse : EraResponse<List<EraCharacterDto>>
{
}
