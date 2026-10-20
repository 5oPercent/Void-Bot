using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Skill;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>转交技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class ListEraSkillPublicationsRequestHandler : IRequestHandler<ListEraSkillPublicationsRequest,ListEraSkillPublicationsResponse>
{
    private readonly IEraSkillService _service;

    public ListEraSkillPublicationsRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<ListEraSkillPublicationsResponse> Handle(IReceiveContext<ListEraSkillPublicationsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ListPublicationsAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
