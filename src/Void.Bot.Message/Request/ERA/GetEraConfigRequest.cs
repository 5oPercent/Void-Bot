using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraConfigRequest : IRequest
{
}

public class GetEraConfigResponse : EraResponse<EraConfigDto>
{
}
