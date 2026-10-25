using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponSlotTypesRequestHandler : IRequestHandler<GetEraWeaponSlotTypesRequest, GetEraWeaponSlotTypesResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponSlotTypesRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponSlotTypesResponse> Handle(IReceiveContext<GetEraWeaponSlotTypesRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetSlotTypesAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
