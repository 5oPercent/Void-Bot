using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraArmorSlotTypeCommandHandler : ICommandHandler<DeleteEraArmorSlotTypeCommand, DeleteEraArmorSlotTypeResponse>
{
    private readonly IEraArmorService _service;

    public DeleteEraArmorSlotTypeCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<DeleteEraArmorSlotTypeResponse> Handle(IReceiveContext<DeleteEraArmorSlotTypeCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteSlotTypeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
