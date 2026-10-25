using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponSlotConfigurationsRequestHandler : IRequestHandler<GetEraWeaponSlotConfigurationsRequest, GetEraWeaponSlotConfigurationsResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponSlotConfigurationsRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponSlotConfigurationsResponse> Handle(IReceiveContext<GetEraWeaponSlotConfigurationsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetSlotConfigurationsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
