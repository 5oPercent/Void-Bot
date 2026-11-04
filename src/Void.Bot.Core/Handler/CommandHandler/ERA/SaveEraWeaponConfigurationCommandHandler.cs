using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraWeaponConfigurationCommandHandler : ICommandHandler<SaveEraWeaponConfigurationCommand, SaveEraWeaponConfigurationResponse>
{
    private readonly IEraWeaponService _service;

    public SaveEraWeaponConfigurationCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<SaveEraWeaponConfigurationResponse> Handle(IReceiveContext<SaveEraWeaponConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.PublishConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
