using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DeleteEraWeaponTemplateCommandHandler : ICommandHandler<DeleteEraWeaponTemplateCommand, DeleteEraWeaponTemplateResponse>
{
    private readonly IEraWeaponService _service;

    public DeleteEraWeaponTemplateCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<DeleteEraWeaponTemplateResponse> Handle(IReceiveContext<DeleteEraWeaponTemplateCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteTemplateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
