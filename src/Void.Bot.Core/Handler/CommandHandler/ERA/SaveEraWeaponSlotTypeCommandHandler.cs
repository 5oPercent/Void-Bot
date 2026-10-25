using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraWeaponSlotTypeCommandHandler : ICommandHandler<SaveEraWeaponSlotTypeCommand, SaveEraWeaponSlotTypeResponse>
{
    private readonly IEraWeaponService _service;

    public SaveEraWeaponSlotTypeCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<SaveEraWeaponSlotTypeResponse> Handle(IReceiveContext<SaveEraWeaponSlotTypeCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveSlotTypeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
