using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraEquipmentManifestRequestHandler : IRequestHandler<GetEraEquipmentManifestRequest, GetEraEquipmentManifestResponse>
{
    private readonly IEraEquipmentService _service;

    public GetEraEquipmentManifestRequestHandler(IEraEquipmentService service)
    {
        _service = service;
    }

    public async Task<GetEraEquipmentManifestResponse> Handle(
        IReceiveContext<GetEraEquipmentManifestRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetManifestAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
