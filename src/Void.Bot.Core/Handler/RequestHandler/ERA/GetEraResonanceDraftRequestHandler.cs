using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>谐振技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class GetEraResonanceDraftRequestHandler : IRequestHandler<GetEraResonanceDraftRequest, GetEraResonanceDraftResponse>
{
    private readonly IEraResonanceService _service;

    public GetEraResonanceDraftRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<GetEraResonanceDraftResponse> Handle(IReceiveContext<GetEraResonanceDraftRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDraftStatusAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
