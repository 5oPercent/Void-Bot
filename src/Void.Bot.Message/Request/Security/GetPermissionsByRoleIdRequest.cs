using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetPermissionsByRoleIdRequest : IRequest
{
    public Guid RoleId { get; set; }
}

public class GetPermissionsByRoleIdResponse : VoidBotResponse<List<PermissionDto>>
{
}