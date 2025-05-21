using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetRoleListRequest : IRequest
{
    public bool ConvertTree { get; set; }

    public List<Guid> IdList { get; set; } = [];
}

public class GetRoleListResponse : VoidBotResponse<List<RoleDto>>
{
}