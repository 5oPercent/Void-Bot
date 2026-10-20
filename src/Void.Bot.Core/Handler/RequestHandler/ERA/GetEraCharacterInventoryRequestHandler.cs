using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraCharacterInventoryRequestHandler : IRequestHandler<GetEraCharacterInventoryRequest, GetEraCharacterInventoryResponse>
{
    private readonly IEraEquipmentService _service;

    public GetEraCharacterInventoryRequestHandler(IEraEquipmentService service)
    {
        _service = service;
    }

    public async Task<GetEraCharacterInventoryResponse> Handle(
        IReceiveContext<GetEraCharacterInventoryRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetInventoryAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
