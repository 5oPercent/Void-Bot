using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraArmorModificationCommandHandler : ICommandHandler<DeleteEraArmorModificationCommand, DeleteEraArmorModificationResponse>
{
    private readonly IEraArmorService _service;

    public DeleteEraArmorModificationCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<DeleteEraArmorModificationResponse> Handle(IReceiveContext<DeleteEraArmorModificationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteModificationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
