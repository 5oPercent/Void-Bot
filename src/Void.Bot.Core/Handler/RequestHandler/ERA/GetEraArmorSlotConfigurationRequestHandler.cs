using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraArmorSlotConfigurationRequestHandler : IRequestHandler<GetEraArmorSlotConfigurationRequest, GetEraArmorSlotConfigurationResponse>
{
    private readonly IEraArmorService _service;

    public GetEraArmorSlotConfigurationRequestHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<GetEraArmorSlotConfigurationResponse> Handle(IReceiveContext<GetEraArmorSlotConfigurationRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetSlotConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
