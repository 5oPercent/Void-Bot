using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraWeaponSlotConfigurationCommandHandler : ICommandHandler<SaveEraWeaponSlotConfigurationCommand, SaveEraWeaponSlotConfigurationResponse>
{
    private readonly IEraWeaponService _service;

    public SaveEraWeaponSlotConfigurationCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<SaveEraWeaponSlotConfigurationResponse> Handle(IReceiveContext<SaveEraWeaponSlotConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveSlotConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
