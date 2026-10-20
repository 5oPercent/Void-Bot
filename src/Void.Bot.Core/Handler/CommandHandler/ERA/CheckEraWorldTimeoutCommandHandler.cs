using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class CheckEraWorldTimeoutCommandHandler : ICommandHandler<CheckEraWorldTimeoutCommand, CheckEraWorldTimeoutResponse>
{
    private readonly IEraWorldService _service;

    public CheckEraWorldTimeoutCommandHandler(IEraWorldService service) { _service = service; }

    public Task<CheckEraWorldTimeoutResponse> Handle(IReceiveContext<CheckEraWorldTimeoutCommand> context, CancellationToken cancellationToken) =>
        _service.CheckTimeoutAsync(context.Message, cancellationToken);
}
