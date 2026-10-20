using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class EndEraWorldCommandHandler : ICommandHandler<EndEraWorldCommand, EndEraWorldResponse>
{
    private readonly IEraWorldService _service;

    public EndEraWorldCommandHandler(IEraWorldService service)
    {
        _service = service;
    }

    public Task<EndEraWorldResponse> Handle(IReceiveContext<EndEraWorldCommand> context, CancellationToken cancellationToken)
    {
        return _service.ExecuteAsync<EndEraWorldResponse>(context.Message, cancellationToken);
    }
}
