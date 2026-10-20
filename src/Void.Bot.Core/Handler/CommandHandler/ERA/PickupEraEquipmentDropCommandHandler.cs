using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Drop;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class PickupEraEquipmentDropCommandHandler : ICommandHandler<PickupEraEquipmentDropCommand, PickupEraEquipmentDropResponse>
{
    private readonly IEraEquipmentDropService _service;

    public PickupEraEquipmentDropCommandHandler(IEraEquipmentDropService service)
    {
        _service = service;
    }

    public Task<PickupEraEquipmentDropResponse> Handle(IReceiveContext<PickupEraEquipmentDropCommand> context, CancellationToken cancellationToken)
    {
        return _service.PickupAsync(context.Message, cancellationToken);
    }
}
