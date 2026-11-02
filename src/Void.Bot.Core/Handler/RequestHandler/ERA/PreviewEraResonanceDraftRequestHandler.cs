using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>转交谐振技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class PreviewEraResonanceDraftRequestHandler : IRequestHandler<PreviewEraResonanceDraftRequest,PreviewEraResonanceDraftResponse>
{
    private readonly IEraResonanceService _service;

    public PreviewEraResonanceDraftRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<PreviewEraResonanceDraftResponse> Handle(IReceiveContext<PreviewEraResonanceDraftRequest> context, CancellationToken cancellationToken)
    {
        return await _service.PreviewDraftAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
