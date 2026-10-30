using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraWeaponFrameworkCommandHandler : ICommandHandler<SaveEraWeaponFrameworkCommand, SaveEraWeaponFrameworkResponse>
{
    private readonly IEraWeaponService _service;

    public SaveEraWeaponFrameworkCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<SaveEraWeaponFrameworkResponse> Handle(IReceiveContext<SaveEraWeaponFrameworkCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveFrameworkAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
