using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Enemy;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>敌人配置消息转交业务服务，事务沿用工作单元。</summary>
public sealed class GetEraDataEnemyRequestHandler : IRequestHandler<GetEraDataEnemyRequest, GetEraDataEnemyResponse>
{
    private readonly IEraEnemyService _service;

    public GetEraDataEnemyRequestHandler(IEraEnemyService service)
    {
        _service = service;
    }

    public async Task<GetEraDataEnemyResponse> Handle(IReceiveContext<GetEraDataEnemyRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDataEnemyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
