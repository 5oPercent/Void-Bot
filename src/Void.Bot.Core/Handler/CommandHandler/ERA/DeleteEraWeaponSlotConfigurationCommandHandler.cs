using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraWeaponSlotConfigurationCommandHandler : ICommandHandler<DeleteEraWeaponSlotConfigurationCommand, DeleteEraWeaponSlotConfigurationResponse>
{
    private readonly IEraWeaponService _service;

    public DeleteEraWeaponSlotConfigurationCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<DeleteEraWeaponSlotConfigurationResponse> Handle(IReceiveContext<DeleteEraWeaponSlotConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteSlotConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
