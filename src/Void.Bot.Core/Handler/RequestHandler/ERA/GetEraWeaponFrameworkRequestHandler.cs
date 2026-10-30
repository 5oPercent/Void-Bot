using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponFrameworkRequestHandler : IRequestHandler<GetEraWeaponFrameworkRequest, GetEraWeaponFrameworkResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponFrameworkRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponFrameworkResponse> Handle(IReceiveContext<GetEraWeaponFrameworkRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetFrameworkAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
