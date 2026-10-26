using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraArmorModificationCommandHandler : ICommandHandler<SaveEraArmorModificationCommand, SaveEraArmorModificationResponse>
{
    private readonly IEraArmorService _service;

    public SaveEraArmorModificationCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<SaveEraArmorModificationResponse> Handle(IReceiveContext<SaveEraArmorModificationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveModificationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
