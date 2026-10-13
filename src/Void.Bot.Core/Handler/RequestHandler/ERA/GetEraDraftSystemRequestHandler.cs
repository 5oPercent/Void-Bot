using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class GetEraDraftSystemRequestHandler : IRequestHandler<GetEraDraftSystemRequest, GetEraDraftSystemResponse>
{
    private readonly IEraSkillService _service;

    public GetEraDraftSystemRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<GetEraDraftSystemResponse> Handle(IReceiveContext<GetEraDraftSystemRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDraftSystemAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
