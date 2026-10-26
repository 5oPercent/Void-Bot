using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraArmorTemplateCommandHandler : ICommandHandler<DeleteEraArmorTemplateCommand, DeleteEraArmorTemplateResponse>
{
    private readonly IEraArmorService _service;

    public DeleteEraArmorTemplateCommandHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<DeleteEraArmorTemplateResponse> Handle(IReceiveContext<DeleteEraArmorTemplateCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteTemplateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
