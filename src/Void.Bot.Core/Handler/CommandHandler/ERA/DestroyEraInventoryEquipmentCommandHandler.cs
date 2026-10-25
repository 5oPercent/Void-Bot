using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DestroyEraInventoryEquipmentCommandHandler : ICommandHandler<DestroyEraInventoryEquipmentCommand, DestroyEraInventoryEquipmentResponse>
{
    private readonly IEraEquipmentService _service;

    public DestroyEraInventoryEquipmentCommandHandler(IEraEquipmentService service)
    {
        _service = service;
    }

    public async Task<DestroyEraInventoryEquipmentResponse> Handle(
        IReceiveContext<DestroyEraInventoryEquipmentCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DestroyInventoryAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
