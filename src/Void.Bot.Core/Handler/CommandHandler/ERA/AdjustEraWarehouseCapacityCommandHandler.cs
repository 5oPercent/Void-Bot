using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class AdjustEraWarehouseCapacityCommandHandler : ICommandHandler<AdjustEraWarehouseCapacityCommand, AdjustEraWarehouseCapacityResponse>
{
    private readonly IEraService _service;

    public AdjustEraWarehouseCapacityCommandHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<AdjustEraWarehouseCapacityResponse> Handle(
        IReceiveContext<AdjustEraWarehouseCapacityCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .AdjustCapacityAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
