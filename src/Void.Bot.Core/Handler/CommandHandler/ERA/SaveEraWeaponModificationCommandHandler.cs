using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraWeaponModificationCommandHandler : ICommandHandler<SaveEraWeaponModificationCommand, SaveEraWeaponModificationResponse>
{
    private readonly IEraWeaponService _service;

    public SaveEraWeaponModificationCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<SaveEraWeaponModificationResponse> Handle(IReceiveContext<SaveEraWeaponModificationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveModificationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
