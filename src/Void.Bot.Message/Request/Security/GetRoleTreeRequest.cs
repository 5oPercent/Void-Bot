using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetRoleTreeRequest : IRequest
{
}

public class GetRoleTreeResponse : VoidBotResponse<List<RoleDto>>
{
}