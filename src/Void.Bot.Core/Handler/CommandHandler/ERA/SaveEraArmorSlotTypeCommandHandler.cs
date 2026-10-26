using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraArmorSlotTypeCommandHandler : ICommandHandler<SaveEraArmorSlotTypeCommand, SaveEraArmorSlotTypeResponse>
{
    private readonly IEraArmorService _service;

    public SaveEraArmorSlotTypeCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<SaveEraArmorSlotTypeResponse> Handle(IReceiveContext<SaveEraArmorSlotTypeCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveSlotTypeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
