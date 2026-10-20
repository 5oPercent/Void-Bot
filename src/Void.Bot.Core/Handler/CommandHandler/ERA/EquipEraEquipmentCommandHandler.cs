using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class EquipEraEquipmentCommandHandler : ICommandHandler<EquipEraEquipmentCommand, EquipEraEquipmentResponse>
{
    private readonly IEraEquipmentService _service;

    public EquipEraEquipmentCommandHandler(IEraEquipmentService service)
    {
        _service = service;
    }

    public async Task<EquipEraEquipmentResponse> Handle(
        IReceiveContext<EquipEraEquipmentCommand> context, CancellationToken cancellationToken)
    {
        return await _service.EquipAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
