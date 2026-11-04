using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>当前配置目录只读查询，不修改业务定义。</summary>
public sealed class ExportEraResonanceDataRequestHandler : IRequestHandler<ExportEraResonanceDataRequest, ExportEraResonanceDataResponse>
{
    private readonly IEraAbilityCatalogQueryService _service;

    public ExportEraResonanceDataRequestHandler(IEraAbilityCatalogQueryService service)
    {
        _service = service;
    }

    public async Task<ExportEraResonanceDataResponse> Handle(IReceiveContext<ExportEraResonanceDataRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ExportDataAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
