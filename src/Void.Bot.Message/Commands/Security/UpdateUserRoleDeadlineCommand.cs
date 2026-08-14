using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class UpdateUserRoleDeadlineCommand : ICommand
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTimeOffset? Deadline { get; set; }
}

public class UpdateUserRoleDeadlineResponse : VoidBotResponse
{
}