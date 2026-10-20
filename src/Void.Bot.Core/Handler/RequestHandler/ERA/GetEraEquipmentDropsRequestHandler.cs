using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraEquipmentDropsRequestHandler : IRequestHandler<GetEraEquipmentDropsRequest, GetEraEquipmentDropsResponse>
{
    private readonly IEraEquipmentDropService _service;

    public GetEraEquipmentDropsRequestHandler(IEraEquipmentDropService service)
    {
        _service = service;
    }

    public Task<GetEraEquipmentDropsResponse> Handle(IReceiveContext<GetEraEquipmentDropsRequest> context, CancellationToken cancellationToken)
    {
        return _service.GetAsync(context.Message, cancellationToken);
    }
}
