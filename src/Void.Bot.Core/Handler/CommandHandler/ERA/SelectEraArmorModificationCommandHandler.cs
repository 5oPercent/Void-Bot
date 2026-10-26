using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SelectEraArmorModificationCommandHandler : ICommandHandler<SelectEraArmorModificationCommand, SelectEraArmorModificationResponse>
{
    private readonly IEraEquipmentService _service;

    public SelectEraArmorModificationCommandHandler(IEraEquipmentService service)
    {
        _service = service;
    }

    public async Task<SelectEraArmorModificationResponse> Handle(IReceiveContext<SelectEraArmorModificationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SelectArmorModificationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
