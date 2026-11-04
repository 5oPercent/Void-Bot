using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraArmorConfigurationCommandHandler : ICommandHandler<SaveEraArmorConfigurationCommand, SaveEraArmorConfigurationResponse>
{
    private readonly IEraArmorService _service;

    public SaveEraArmorConfigurationCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<SaveEraArmorConfigurationResponse> Handle(IReceiveContext<SaveEraArmorConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.PublishConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
