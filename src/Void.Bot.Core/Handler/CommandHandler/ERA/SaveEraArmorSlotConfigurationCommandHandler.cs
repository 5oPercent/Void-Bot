using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraArmorSlotConfigurationCommandHandler : ICommandHandler<SaveEraArmorSlotConfigurationCommand, SaveEraArmorSlotConfigurationResponse>
{
    private readonly IEraArmorService _service;

    public SaveEraArmorSlotConfigurationCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<SaveEraArmorSlotConfigurationResponse> Handle(IReceiveContext<SaveEraArmorSlotConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveSlotConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
