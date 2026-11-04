using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>转交谐振技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class GetEraDataReferencesRequestHandler : IRequestHandler<GetEraDataReferencesRequest,GetEraDataReferencesResponse>
{
    private readonly IEraAbilityCatalogQueryService _service;

    public GetEraDataReferencesRequestHandler(IEraAbilityCatalogQueryService service)
    {
        _service = service;
    }

    public async Task<GetEraDataReferencesResponse> Handle(IReceiveContext<GetEraDataReferencesRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDataReferencesAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
