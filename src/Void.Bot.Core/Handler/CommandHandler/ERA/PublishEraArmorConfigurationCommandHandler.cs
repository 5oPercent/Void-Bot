using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class PublishEraArmorConfigurationCommandHandler : ICommandHandler<PublishEraArmorConfigurationCommand, PublishEraArmorConfigurationResponse>
{
    private readonly IEraArmorService _service;

    public PublishEraArmorConfigurationCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<PublishEraArmorConfigurationResponse> Handle(IReceiveContext<PublishEraArmorConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.PublishConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
