using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWorldEquipmentRequestHandler : IRequestHandler<GetEraWorldEquipmentRequest, GetEraWorldEquipmentResponse>
{
    private readonly IEraWorldEquipmentService _service;

    public GetEraWorldEquipmentRequestHandler(IEraWorldEquipmentService service)
    {
        _service = service;
    }

    public async Task<GetEraWorldEquipmentResponse> Handle(IReceiveContext<GetEraWorldEquipmentRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
