using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class ReplaceEndpointListCommandHandler : ICommandHandler<ReplaceEndpointListCommand>
{
    private readonly IEndpointService _service;

    public ReplaceEndpointListCommandHandler(IEndpointService service)
    {
        _service = service;
    }

    public async Task Handle(IReceiveContext<ReplaceEndpointListCommand> context, CancellationToken cancellationToken)
    {
        await _service.ReplaceEndpointListAsync(context.Message).ConfigureAwait(false);
    }
}