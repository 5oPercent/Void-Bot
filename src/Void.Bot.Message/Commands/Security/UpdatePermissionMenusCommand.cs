using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class UpdatePermissionMenusCommand : ICommand
{
    public Guid PermissionId { get; set; }

    public List<Guid> MenuIds { get; set; } = [];
}

public class UpdatePermissionMenusResponse : VoidBotResponse
{
}