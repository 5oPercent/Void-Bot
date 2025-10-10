using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class DeleteRoleConflictMatrixCommand : ICommand
{
    public Guid Id { get; set; }
}

public class DeleteRoleConflictMatrixResponse : VoidBotResponse
{
}