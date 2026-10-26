using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraArmorSlotConfigurationsRequestHandler : IRequestHandler<GetEraArmorSlotConfigurationsRequest, GetEraArmorSlotConfigurationsResponse>
{
    private readonly IEraArmorService _service;

    public GetEraArmorSlotConfigurationsRequestHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<GetEraArmorSlotConfigurationsResponse> Handle(IReceiveContext<GetEraArmorSlotConfigurationsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetSlotConfigurationsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
