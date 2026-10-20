using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Skill;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>转交技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class PreviewEraSkillDraftRequestHandler : IRequestHandler<PreviewEraSkillDraftRequest,PreviewEraSkillDraftResponse>
{
    private readonly IEraSkillService _service;

    public PreviewEraSkillDraftRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<PreviewEraSkillDraftResponse> Handle(IReceiveContext<PreviewEraSkillDraftRequest> context, CancellationToken cancellationToken)
    {
        return await _service.PreviewDraftAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
