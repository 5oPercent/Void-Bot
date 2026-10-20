using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class ReadyEraWorldCommandHandler : ICommandHandler<ReadyEraWorldCommand, ReadyEraWorldResponse>
{
    private readonly IEraWorldService _service;

    public ReadyEraWorldCommandHandler(IEraWorldService service)
    {
        _service = service;
    }

    public Task<ReadyEraWorldResponse> Handle(IReceiveContext<ReadyEraWorldCommand> context, CancellationToken cancellationToken)
    {
        return _service.ExecuteAsync<ReadyEraWorldResponse>(context.Message, cancellationToken);
    }
}
