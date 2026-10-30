using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponFrameworksRequestHandler : IRequestHandler<GetEraWeaponFrameworksRequest, GetEraWeaponFrameworksResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponFrameworksRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponFrameworksResponse> Handle(IReceiveContext<GetEraWeaponFrameworksRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetFrameworksAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
