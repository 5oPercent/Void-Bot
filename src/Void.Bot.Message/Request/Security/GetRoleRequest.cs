using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetRoleRequest : IRequest
{
    public Guid Id { get; set; }
}

public class GetRoleResponse : VoidBotResponse<RoleDto>
{
}