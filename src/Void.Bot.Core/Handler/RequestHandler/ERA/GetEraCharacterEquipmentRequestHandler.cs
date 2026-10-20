using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraCharacterEquipmentRequestHandler : IRequestHandler<GetEraCharacterEquipmentRequest, GetEraCharacterEquipmentResponse>
{
    private readonly IEraEquipmentService _service;

    public GetEraCharacterEquipmentRequestHandler(IEraEquipmentService service)
    {
        _service = service;
    }

    public async Task<GetEraCharacterEquipmentResponse> Handle(
        IReceiveContext<GetEraCharacterEquipmentRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetEquipmentAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
