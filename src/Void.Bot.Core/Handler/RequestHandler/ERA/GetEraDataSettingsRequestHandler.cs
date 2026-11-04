using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.CombatRelation;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>种族阵营关系配置消息转交本类别业务服务，事务沿用工作单元。</summary>
public sealed class GetEraDataSettingsRequestHandler : IRequestHandler<GetEraDataSettingsRequest, GetEraDataSettingsResponse>
{
    private readonly IEraCombatRelationService _service;

    public GetEraDataSettingsRequestHandler(IEraCombatRelationService service)
    {
        _service = service;
    }

    public async Task<GetEraDataSettingsResponse> Handle(IReceiveContext<GetEraDataSettingsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDataSettingsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
