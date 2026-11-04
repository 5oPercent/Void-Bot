using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Effect;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>效果配置消息转交本类别业务服务，事务沿用工作单元。</summary>
public sealed class ListEraDataBuffRequestHandler : IRequestHandler<ListEraDataBuffRequest, ListEraDataBuffResponse>
{
    private readonly IEraEffectService _service;

    public ListEraDataBuffRequestHandler(IEraEffectService service)
    {
        _service = service;
    }

    public async Task<ListEraDataBuffResponse> Handle(IReceiveContext<ListEraDataBuffRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ListDataBuffAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
