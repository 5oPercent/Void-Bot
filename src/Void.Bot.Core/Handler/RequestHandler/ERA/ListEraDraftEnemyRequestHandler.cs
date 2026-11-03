using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>敌人配置草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class ListEraDraftEnemyRequestHandler : IRequestHandler<ListEraDraftEnemyRequest, ListEraDraftEnemyResponse>
{
    private readonly IEraResonanceService _service;

    public ListEraDraftEnemyRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<ListEraDraftEnemyResponse> Handle(IReceiveContext<ListEraDraftEnemyRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ListDraftEnemyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
