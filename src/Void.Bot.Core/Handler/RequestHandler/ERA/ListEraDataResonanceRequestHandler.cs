using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>谐振技能配置消息转交业务服务，事务沿用工作单元。</summary>
public sealed class ListEraDataResonanceRequestHandler : IRequestHandler<ListEraDataResonanceRequest, ListEraDataResonanceResponse>
{
    private readonly IEraResonanceService _service;

    public ListEraDataResonanceRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<ListEraDataResonanceResponse> Handle(IReceiveContext<ListEraDataResonanceRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ListDataResonanceAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
