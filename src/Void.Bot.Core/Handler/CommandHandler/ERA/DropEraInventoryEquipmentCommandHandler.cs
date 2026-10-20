using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Drop;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DropEraInventoryEquipmentCommandHandler : ICommandHandler<DropEraInventoryEquipmentCommand, DropEraInventoryEquipmentResponse>
{
    private readonly IEraEquipmentDropService _service;

    public DropEraInventoryEquipmentCommandHandler(IEraEquipmentDropService service)
    {
        _service = service;
    }

    public Task<DropEraInventoryEquipmentResponse> Handle(IReceiveContext<DropEraInventoryEquipmentCommand> context, CancellationToken cancellationToken)
    {
        return _service.DropAsync(context.Message, cancellationToken);
    }
}
