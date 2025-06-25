using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Commands.Security;

public class ReplaceEndpointListCommand : ICommand
{
    public List<EndpointDto> Endpoints { get; set; } = [];
}