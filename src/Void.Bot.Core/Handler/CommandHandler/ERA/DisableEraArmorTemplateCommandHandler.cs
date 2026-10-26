using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DisableEraArmorTemplateCommandHandler : ICommandHandler<DisableEraArmorTemplateCommand, DisableEraArmorTemplateResponse>
{
    private readonly IEraArmorService _service;

    public DisableEraArmorTemplateCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<DisableEraArmorTemplateResponse> Handle(IReceiveContext<DisableEraArmorTemplateCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DisableTemplateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
