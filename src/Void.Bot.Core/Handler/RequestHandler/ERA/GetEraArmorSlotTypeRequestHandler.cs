using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraArmorSlotTypeRequestHandler : IRequestHandler<GetEraArmorSlotTypeRequest, GetEraArmorSlotTypeResponse>
{
    private readonly IEraArmorService _service;

    public GetEraArmorSlotTypeRequestHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<GetEraArmorSlotTypeResponse> Handle(IReceiveContext<GetEraArmorSlotTypeRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetSlotTypeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
