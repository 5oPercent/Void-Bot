using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraArmorModificationRequestHandler : IRequestHandler<GetEraArmorModificationRequest, GetEraArmorModificationResponse>
{
    private readonly IEraArmorService _service;

    public GetEraArmorModificationRequestHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<GetEraArmorModificationResponse> Handle(IReceiveContext<GetEraArmorModificationRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetModificationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
