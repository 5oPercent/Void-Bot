using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class RecoverEraWorldCommandHandler : ICommandHandler<RecoverEraWorldCommand, RecoverEraWorldResponse>
{
    private readonly IEraWorldService _service;

    public RecoverEraWorldCommandHandler(IEraWorldService service)
    {
        _service = service;
    }

    public Task<RecoverEraWorldResponse> Handle(IReceiveContext<RecoverEraWorldCommand> context, CancellationToken cancellationToken)
    {
        return _service.ExecuteAsync<RecoverEraWorldResponse>(context.Message, cancellationToken);
    }
}
