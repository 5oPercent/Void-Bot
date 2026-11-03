using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>敌人配置草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class GetEraDraftEnemyRequestHandler : IRequestHandler<GetEraDraftEnemyRequest, GetEraDraftEnemyResponse>
{
    private readonly IEraResonanceService _service;

    public GetEraDraftEnemyRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<GetEraDraftEnemyResponse> Handle(IReceiveContext<GetEraDraftEnemyRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDraftEnemyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
