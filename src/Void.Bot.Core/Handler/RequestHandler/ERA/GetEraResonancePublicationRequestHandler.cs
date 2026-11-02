using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>转交谐振技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class GetEraResonancePublicationRequestHandler : IRequestHandler<GetEraResonancePublicationRequest,GetEraResonancePublicationResponse>
{
    private readonly IEraResonanceService _service;

    public GetEraResonancePublicationRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<GetEraResonancePublicationResponse> Handle(IReceiveContext<GetEraResonancePublicationRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetPublicationAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
