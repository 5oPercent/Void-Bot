using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Request.Security;

public class GetPermissionsByRoleIdRequest : IRequest
{
    public Guid RoleId { get; set; }
}

public class GetPermissionsByRoleIdResponse : VoidBotResponse<List<Guid>>
{
}