using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>转交谐振技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class ListEraResonancePublicationsRequestHandler : IRequestHandler<ListEraResonancePublicationsRequest,ListEraResonancePublicationsResponse>
{
    private readonly IEraResonanceService _service;

    public ListEraResonancePublicationsRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<ListEraResonancePublicationsResponse> Handle(IReceiveContext<ListEraResonancePublicationsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ListPublicationsAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
