using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class PublishEraWeaponConfigurationCommandHandler : ICommandHandler<PublishEraWeaponConfigurationCommand, PublishEraWeaponConfigurationResponse>
{
    private readonly IEraWeaponService _service;

    public PublishEraWeaponConfigurationCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<PublishEraWeaponConfigurationResponse> Handle(IReceiveContext<PublishEraWeaponConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.PublishConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
