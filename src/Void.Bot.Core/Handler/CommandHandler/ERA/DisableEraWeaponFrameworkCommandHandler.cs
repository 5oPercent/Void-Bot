using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DisableEraWeaponFrameworkCommandHandler : ICommandHandler<DisableEraWeaponFrameworkCommand, DisableEraWeaponFrameworkResponse>
{
    private readonly IEraWeaponService _service;

    public DisableEraWeaponFrameworkCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<DisableEraWeaponFrameworkResponse> Handle(IReceiveContext<DisableEraWeaponFrameworkCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DisableFrameworkAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
