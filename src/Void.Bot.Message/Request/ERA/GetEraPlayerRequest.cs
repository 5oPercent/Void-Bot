using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraPlayerRequest : IRequest
{
}

public class GetEraPlayerResponse : EraResponse<EraPlayerDto>
{
}
