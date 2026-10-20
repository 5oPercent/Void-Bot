using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Skill;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class GetEraDraftSkillRequestHandler : IRequestHandler<GetEraDraftSkillRequest, GetEraDraftSkillResponse>
{
    private readonly IEraSkillService _service;

    public GetEraDraftSkillRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<GetEraDraftSkillResponse> Handle(IReceiveContext<GetEraDraftSkillRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDraftSkillAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
