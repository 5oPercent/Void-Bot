using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetEndpointListRequest : IRequest
{
}

public class GetEndpointListResponse : VoidBotResponse<List<EndpointDto>>
{
}