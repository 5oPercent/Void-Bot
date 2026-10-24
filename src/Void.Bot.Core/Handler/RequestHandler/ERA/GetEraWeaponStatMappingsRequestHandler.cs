using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponStatMappingsRequestHandler : IRequestHandler<GetEraWeaponStatMappingsRequest, GetEraWeaponStatMappingsResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponStatMappingsRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponStatMappingsResponse> Handle(IReceiveContext<GetEraWeaponStatMappingsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetStatMappingsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
