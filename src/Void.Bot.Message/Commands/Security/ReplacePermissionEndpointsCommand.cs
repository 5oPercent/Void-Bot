using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class ReplacePermissionEndpointsCommand : ICommand
{
    public Guid PermissionId { get; set; }

    public List<string> Endpoints { get; set; } = [];
}

public class ReplacePermissionEndpointsResponse : VoidBotResponse
{
}