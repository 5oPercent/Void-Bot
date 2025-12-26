using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Commands.Security;

public class UpdatePermissionCommand : PermissionBaseDto, ICommand
{
    public Guid Id { get; set; }
}

public class UpdatePermissionResponse : VoidBotResponse
{
}