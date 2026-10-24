using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SelectEraWeaponModificationCommandHandler : ICommandHandler<SelectEraWeaponModificationCommand, SelectEraWeaponModificationResponse>
{
    private readonly IEraEquipmentService _service;

    public SelectEraWeaponModificationCommandHandler(IEraEquipmentService service)
    {
        _service = service;
    }

    public async Task<SelectEraWeaponModificationResponse> Handle(IReceiveContext<SelectEraWeaponModificationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SelectWeaponModificationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
