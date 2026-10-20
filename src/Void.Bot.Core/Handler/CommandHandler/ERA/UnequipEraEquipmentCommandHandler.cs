using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class UnequipEraEquipmentCommandHandler : ICommandHandler<UnequipEraEquipmentCommand, UnequipEraEquipmentResponse>
{
    private readonly IEraEquipmentService _service;

    public UnequipEraEquipmentCommandHandler(IEraEquipmentService service)
    {
        _service = service;
    }

    public async Task<UnequipEraEquipmentResponse> Handle(
        IReceiveContext<UnequipEraEquipmentCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UnequipAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
