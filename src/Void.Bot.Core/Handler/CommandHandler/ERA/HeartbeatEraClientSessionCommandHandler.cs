using System.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class HeartbeatEraClientSessionCommandHandler : ICommandHandler<HeartbeatEraClientSessionCommand, HeartbeatEraClientSessionResponse>
{
    private readonly IEraClientSessionService _service;

    public HeartbeatEraClientSessionCommandHandler(IEraClientSessionService service)
    {
        _service = service;
    }

    public async Task<HeartbeatEraClientSessionResponse> Handle(IReceiveContext<HeartbeatEraClientSessionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.HeartbeatAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
