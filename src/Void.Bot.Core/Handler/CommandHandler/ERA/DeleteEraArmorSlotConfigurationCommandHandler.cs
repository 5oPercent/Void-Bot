using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraArmorSlotConfigurationCommandHandler : ICommandHandler<DeleteEraArmorSlotConfigurationCommand, DeleteEraArmorSlotConfigurationResponse>
{
    private readonly IEraArmorService _service;

    public DeleteEraArmorSlotConfigurationCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<DeleteEraArmorSlotConfigurationResponse> Handle(IReceiveContext<DeleteEraArmorSlotConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteSlotConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
