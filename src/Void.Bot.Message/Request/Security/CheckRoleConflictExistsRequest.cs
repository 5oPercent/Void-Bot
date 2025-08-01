using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Request.Security;

public class CheckRoleConflictExistsRequest : IRequest
{
    public Guid RoleAId { get; set; }
    
    public Guid RoleBId { get; set; }
}

public class CheckRoleConflictExistsResponse : VoidBotResponse<bool>
{
}