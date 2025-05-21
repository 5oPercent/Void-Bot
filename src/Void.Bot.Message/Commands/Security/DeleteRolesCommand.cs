using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class DeleteRolesCommand : ICommand
{
    public required List<Guid> IdList { get; set; } = [];
}

public class DeleteRolesResponse : VoidBotResponse
{
}