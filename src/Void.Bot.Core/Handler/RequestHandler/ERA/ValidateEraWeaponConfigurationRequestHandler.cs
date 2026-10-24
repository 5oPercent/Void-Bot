using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class ValidateEraWeaponConfigurationRequestHandler : IRequestHandler<ValidateEraWeaponConfigurationRequest, ValidateEraWeaponConfigurationResponse>
{
    private readonly IEraWeaponService _service;

    public ValidateEraWeaponConfigurationRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<ValidateEraWeaponConfigurationResponse> Handle(IReceiveContext<ValidateEraWeaponConfigurationRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ValidateConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
