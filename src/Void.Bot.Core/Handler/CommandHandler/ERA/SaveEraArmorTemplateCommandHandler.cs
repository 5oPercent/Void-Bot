using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraArmorTemplateCommandHandler : ICommandHandler<SaveEraArmorTemplateCommand, SaveEraArmorTemplateResponse>
{
    private readonly IEraArmorService _service;

    public SaveEraArmorTemplateCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<SaveEraArmorTemplateResponse> Handle(IReceiveContext<SaveEraArmorTemplateCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveTemplateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
