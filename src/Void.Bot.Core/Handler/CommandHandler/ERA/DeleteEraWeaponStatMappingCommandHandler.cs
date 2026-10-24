using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraWeaponStatMappingCommandHandler : ICommandHandler<DeleteEraWeaponStatMappingCommand, DeleteEraWeaponStatMappingResponse>
{
    private readonly IEraWeaponService _service;

    public DeleteEraWeaponStatMappingCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<DeleteEraWeaponStatMappingResponse> Handle(IReceiveContext<DeleteEraWeaponStatMappingCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteStatMappingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
