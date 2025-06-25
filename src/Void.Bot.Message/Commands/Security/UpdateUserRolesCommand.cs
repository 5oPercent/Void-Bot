using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class UpdateUserRolesCommand : ICommand
{
    public Guid UserId { get; set; }

    public List<Guid> RoleIds { get; set; } = [];
}

public class UpdateUserRolesResponse : VoidBotResponse
{
}