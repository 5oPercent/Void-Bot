using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraWeaponFrameworkCommandHandler : ICommandHandler<DeleteEraWeaponFrameworkCommand, DeleteEraWeaponFrameworkResponse>
{
    private readonly IEraWeaponService _service;

    public DeleteEraWeaponFrameworkCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<DeleteEraWeaponFrameworkResponse> Handle(IReceiveContext<DeleteEraWeaponFrameworkCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteFrameworkAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
