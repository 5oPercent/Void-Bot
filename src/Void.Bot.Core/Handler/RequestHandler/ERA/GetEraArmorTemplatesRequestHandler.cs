using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraArmorTemplatesRequestHandler : IRequestHandler<GetEraArmorTemplatesRequest, GetEraArmorTemplatesResponse>
{
    private readonly IEraArmorService _service;

    public GetEraArmorTemplatesRequestHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<GetEraArmorTemplatesResponse> Handle(IReceiveContext<GetEraArmorTemplatesRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetTemplatesAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
