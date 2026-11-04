using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Enemy;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>敌人配置消息转交业务服务，事务沿用工作单元。</summary>
public sealed class ListEraDataEnemyRequestHandler : IRequestHandler<ListEraDataEnemyRequest, ListEraDataEnemyResponse>
{
    private readonly IEraEnemyService _service;

    public ListEraDataEnemyRequestHandler(IEraEnemyService service)
    {
        _service = service;
    }

    public async Task<ListEraDataEnemyResponse> Handle(IReceiveContext<ListEraDataEnemyRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ListDataEnemyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
