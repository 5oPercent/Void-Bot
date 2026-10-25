using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraWeaponSlotTypeCommandHandler : ICommandHandler<DeleteEraWeaponSlotTypeCommand, DeleteEraWeaponSlotTypeResponse>
{
    private readonly IEraWeaponService _service;

    public DeleteEraWeaponSlotTypeCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<DeleteEraWeaponSlotTypeResponse> Handle(IReceiveContext<DeleteEraWeaponSlotTypeCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteSlotTypeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
