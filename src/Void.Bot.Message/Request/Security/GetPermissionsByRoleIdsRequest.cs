using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetPermissionsByRoleIdsRequest : IRequest
{
    public List<Guid> RoleIds { get; set; } = [];
}

public class GetPermissionsByRoleIdsResponse : VoidBotResponse<List<PermissionDto>>
{
}