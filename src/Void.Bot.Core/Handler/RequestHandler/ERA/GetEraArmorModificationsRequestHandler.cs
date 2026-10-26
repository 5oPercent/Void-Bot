using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraArmorModificationsRequestHandler : IRequestHandler<GetEraArmorModificationsRequest, GetEraArmorModificationsResponse>
{
    private readonly IEraArmorService _service;

    public GetEraArmorModificationsRequestHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<GetEraArmorModificationsResponse> Handle(IReceiveContext<GetEraArmorModificationsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetModificationsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
