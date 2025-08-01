using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetRolesByUserIdRequest : IRequest
{
    public Guid UserId { get; set; }
}

public class GetRolesByUserIdResponse : VoidBotResponse<GetRolesByUserIdData>
{
}

public class GetRolesByUserIdData
{
    public List<UserRoleDto> UserRoles { get; set; } = [];
    
    public List<RoleDto> Roles { get; set; } = [];
}