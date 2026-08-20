using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetCurrentUserPermissionsRequest : IRequest
{
}

public class GetCurrentUserPermissionsResponse : VoidBotResponse<GetCurrentUserPermissionsData>
{
}

public class GetCurrentUserPermissionsData
{
    public List<AuthorizedRoleDto> Roles { get; set; } = [];

    public List<AuthorizedPermissionDto> Permissions { get; set; } = [];

    public List<AuthorizedMenuDto> Menus { get; set; } = [];
}