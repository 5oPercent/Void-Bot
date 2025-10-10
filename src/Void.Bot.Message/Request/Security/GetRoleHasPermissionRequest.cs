using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Request.Security;

public class GetRoleHasPermissionRequest : IRequest
{
    public Guid RoleId { get; set; }
    
    public Guid PermissionId { get; set; }
}

public class GetRoleHasPermissionResponse : VoidBotResponse<bool>
{
}