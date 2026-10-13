using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class ExportEraSkillDraftRequestHandler : IRequestHandler<ExportEraSkillDraftRequest, ExportEraSkillDraftResponse>
{
    private readonly IEraSkillService _service;

    public ExportEraSkillDraftRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<ExportEraSkillDraftResponse> Handle(IReceiveContext<ExportEraSkillDraftRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ExportDraftAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
