using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponModificationsRequestHandler : IRequestHandler<GetEraWeaponModificationsRequest, GetEraWeaponModificationsResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponModificationsRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponModificationsResponse> Handle(IReceiveContext<GetEraWeaponModificationsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetModificationsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
