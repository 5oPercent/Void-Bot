using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraWeaponStatMappingCommandHandler : ICommandHandler<SaveEraWeaponStatMappingCommand, SaveEraWeaponStatMappingResponse>
{
    private readonly IEraWeaponService _service;

    public SaveEraWeaponStatMappingCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<SaveEraWeaponStatMappingResponse> Handle(IReceiveContext<SaveEraWeaponStatMappingCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveStatMappingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
