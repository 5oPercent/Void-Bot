using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponSlotConfigurationRequestHandler : IRequestHandler<GetEraWeaponSlotConfigurationRequest, GetEraWeaponSlotConfigurationResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponSlotConfigurationRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponSlotConfigurationResponse> Handle(IReceiveContext<GetEraWeaponSlotConfigurationRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetSlotConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
