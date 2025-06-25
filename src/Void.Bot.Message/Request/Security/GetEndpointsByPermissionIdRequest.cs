using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetEndpointsByPermissionIdRequest : IRequest
{
    public Guid PermissionId { get; set; }
}

public class GetEndpointsByPermissionIdResponse : VoidBotResponse<List<EndpointDto>>
{
}