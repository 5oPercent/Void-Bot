using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class ListEraDraftBuffRequestHandler : IRequestHandler<ListEraDraftBuffRequest, ListEraDraftBuffResponse>
{
    private readonly IEraSkillService _service;

    public ListEraDraftBuffRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<ListEraDraftBuffResponse> Handle(IReceiveContext<ListEraDraftBuffRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ListDraftBuffAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
