using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Lattice;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>晶格配置消息转交本类别业务服务，事务沿用工作单元。</summary>
public sealed class ListEraDataLatticeRequestHandler : IRequestHandler<ListEraDataLatticeRequest, ListEraDataLatticeResponse>
{
    private readonly IEraLatticeService _service;

    public ListEraDataLatticeRequestHandler(IEraLatticeService service)
    {
        _service = service;
    }

    public async Task<ListEraDataLatticeResponse> Handle(IReceiveContext<ListEraDataLatticeRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ListDataLatticeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
