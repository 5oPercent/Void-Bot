using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraArmorSlotTypesRequestHandler : IRequestHandler<GetEraArmorSlotTypesRequest, GetEraArmorSlotTypesResponse>
{
    private readonly IEraArmorService _service;

    public GetEraArmorSlotTypesRequestHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<GetEraArmorSlotTypesResponse> Handle(IReceiveContext<GetEraArmorSlotTypesRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetSlotTypesAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
