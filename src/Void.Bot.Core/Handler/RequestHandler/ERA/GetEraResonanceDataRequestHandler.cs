using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>当前配置目录只读查询，不修改业务定义。</summary>
public sealed class GetEraResonanceDataRequestHandler : IRequestHandler<GetEraResonanceDataRequest, GetEraResonanceDataResponse>
{
    private readonly IEraAbilityCatalogQueryService _service;

    public GetEraResonanceDataRequestHandler(IEraAbilityCatalogQueryService service)
    {
        _service = service;
    }

    public async Task<GetEraResonanceDataResponse> Handle(IReceiveContext<GetEraResonanceDataRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDataStatusAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
