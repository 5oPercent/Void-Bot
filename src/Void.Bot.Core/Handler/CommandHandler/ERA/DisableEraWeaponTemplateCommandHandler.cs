using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DisableEraWeaponTemplateCommandHandler : ICommandHandler<DisableEraWeaponTemplateCommand, DisableEraWeaponTemplateResponse>
{
    private readonly IEraWeaponService _service;

    public DisableEraWeaponTemplateCommandHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<DisableEraWeaponTemplateResponse> Handle(IReceiveContext<DisableEraWeaponTemplateCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DisableTemplateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
