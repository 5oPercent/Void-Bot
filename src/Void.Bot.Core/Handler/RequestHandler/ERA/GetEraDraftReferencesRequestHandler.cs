using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>转交谐振技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class GetEraDraftReferencesRequestHandler : IRequestHandler<GetEraDraftReferencesRequest,GetEraDraftReferencesResponse>
{
    private readonly IEraResonanceService _service;

    public GetEraDraftReferencesRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<GetEraDraftReferencesResponse> Handle(IReceiveContext<GetEraDraftReferencesRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDraftReferencesAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
