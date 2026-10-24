using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponStatMappingRequestHandler : IRequestHandler<GetEraWeaponStatMappingRequest, GetEraWeaponStatMappingResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponStatMappingRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponStatMappingResponse> Handle(IReceiveContext<GetEraWeaponStatMappingRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetStatMappingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
