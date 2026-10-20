using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class HeartbeatEraWorldCommandHandler : ICommandHandler<HeartbeatEraWorldCommand, HeartbeatEraWorldResponse>
{
    private readonly IEraWorldService _service;

    public HeartbeatEraWorldCommandHandler(IEraWorldService service)
    {
        _service = service;
    }

    public Task<HeartbeatEraWorldResponse> Handle(IReceiveContext<HeartbeatEraWorldCommand> context, CancellationToken cancellationToken)
    {
        return _service.HeartbeatAsync(context.Message, cancellationToken);
    }
}
