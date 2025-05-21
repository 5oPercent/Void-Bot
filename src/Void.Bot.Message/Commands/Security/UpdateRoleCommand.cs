using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Commands.Security;

public class UpdateRoleCommand : ICommand
{
    public required RoleDto Role { get; set; }
}

public class UpdateRoleResponse : VoidBotResponse
{
}