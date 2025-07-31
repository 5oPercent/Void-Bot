using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetEndpointsByPermissionIdsRequest : IRequest
{
    public List<Guid> PermissionIds { get; set; } = [];
}

public class GetEndpointsByPermissionIdsResponse : VoidBotResponse<List<EndpointDto>>
{
}