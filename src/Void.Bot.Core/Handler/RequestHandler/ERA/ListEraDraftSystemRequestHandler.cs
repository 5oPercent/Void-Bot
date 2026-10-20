using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Skill;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class ListEraDraftSystemRequestHandler : IRequestHandler<ListEraDraftSystemRequest, ListEraDraftSystemResponse>
{
    private readonly IEraSkillService _service;

    public ListEraDraftSystemRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<ListEraDraftSystemResponse> Handle(IReceiveContext<ListEraDraftSystemRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ListDraftSystemAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
