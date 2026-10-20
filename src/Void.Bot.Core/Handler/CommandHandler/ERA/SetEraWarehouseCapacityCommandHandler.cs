using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class SetEraWarehouseCapacityCommandHandler : ICommandHandler<SetEraWarehouseCapacityCommand, SetEraWarehouseCapacityResponse>
{
    private readonly IEraService _service;

    public SetEraWarehouseCapacityCommandHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<SetEraWarehouseCapacityResponse> Handle(
        IReceiveContext<SetEraWarehouseCapacityCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .SetCapacityAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
