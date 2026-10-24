using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraWeaponModificationCommandHandler : ICommandHandler<DeleteEraWeaponModificationCommand, DeleteEraWeaponModificationResponse>
{
    private readonly IEraWeaponService _service;

    public DeleteEraWeaponModificationCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<DeleteEraWeaponModificationResponse> Handle(IReceiveContext<DeleteEraWeaponModificationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteModificationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
