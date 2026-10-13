using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>转交技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class ValidateEraSkillDraftRequestHandler : IRequestHandler<ValidateEraSkillDraftRequest,ValidateEraSkillDraftResponse>
{
    private readonly IEraSkillService _service;

    public ValidateEraSkillDraftRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<ValidateEraSkillDraftResponse> Handle(IReceiveContext<ValidateEraSkillDraftRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ValidateDraftAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
