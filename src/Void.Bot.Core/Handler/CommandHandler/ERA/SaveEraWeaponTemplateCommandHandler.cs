using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraWeaponTemplateCommandHandler : ICommandHandler<SaveEraWeaponTemplateCommand, SaveEraWeaponTemplateResponse>
{
    private readonly IEraWeaponService _service;

    public SaveEraWeaponTemplateCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<SaveEraWeaponTemplateResponse> Handle(IReceiveContext<SaveEraWeaponTemplateCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveTemplateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
