using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class JoinEraWorldCommandHandler : ICommandHandler<JoinEraWorldCommand, JoinEraWorldResponse>
{
    private readonly IEraWorldService _service;

    public JoinEraWorldCommandHandler(IEraWorldService service) { _service = service; }

    public Task<JoinEraWorldResponse> Handle(IReceiveContext<JoinEraWorldCommand> context, CancellationToken cancellationToken) =>
        _service.ExecuteAsync<JoinEraWorldResponse>(context.Message, cancellationToken);
}
