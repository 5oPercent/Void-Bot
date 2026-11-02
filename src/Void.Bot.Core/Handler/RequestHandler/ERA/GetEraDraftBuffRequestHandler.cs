using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

/// <summary>谐振技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class GetEraDraftBuffRequestHandler : IRequestHandler<GetEraDraftBuffRequest, GetEraDraftBuffResponse>
{
    private readonly IEraResonanceService _service;

    public GetEraDraftBuffRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<GetEraDraftBuffResponse> Handle(IReceiveContext<GetEraDraftBuffRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDraftBuffAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
