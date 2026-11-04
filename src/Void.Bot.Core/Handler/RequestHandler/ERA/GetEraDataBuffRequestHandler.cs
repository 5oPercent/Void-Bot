using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Effect;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>效果配置消息转交本类别业务服务，事务沿用工作单元。</summary>
public sealed class GetEraDataBuffRequestHandler : IRequestHandler<GetEraDataBuffRequest, GetEraDataBuffResponse>
{
    private readonly IEraEffectService _service;

    public GetEraDataBuffRequestHandler(IEraEffectService service)
    {
        _service = service;
    }

    public async Task<GetEraDataBuffResponse> Handle(IReceiveContext<GetEraDataBuffRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDataBuffAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
