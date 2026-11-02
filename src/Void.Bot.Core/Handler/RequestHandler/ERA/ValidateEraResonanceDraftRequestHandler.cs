using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>转交谐振技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class ValidateEraResonanceDraftRequestHandler : IRequestHandler<ValidateEraResonanceDraftRequest,ValidateEraResonanceDraftResponse>
{
    private readonly IEraResonanceService _service;

    public ValidateEraResonanceDraftRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<ValidateEraResonanceDraftResponse> Handle(IReceiveContext<ValidateEraResonanceDraftRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ValidateDraftAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
