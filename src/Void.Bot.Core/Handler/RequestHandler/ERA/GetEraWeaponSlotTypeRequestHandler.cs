using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponSlotTypeRequestHandler : IRequestHandler<GetEraWeaponSlotTypeRequest, GetEraWeaponSlotTypeResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponSlotTypeRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponSlotTypeResponse> Handle(IReceiveContext<GetEraWeaponSlotTypeRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetSlotTypeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
