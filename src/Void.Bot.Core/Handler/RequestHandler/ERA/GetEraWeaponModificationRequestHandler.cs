using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponModificationRequestHandler : IRequestHandler<GetEraWeaponModificationRequest, GetEraWeaponModificationResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponModificationRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponModificationResponse> Handle(IReceiveContext<GetEraWeaponModificationRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetModificationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
