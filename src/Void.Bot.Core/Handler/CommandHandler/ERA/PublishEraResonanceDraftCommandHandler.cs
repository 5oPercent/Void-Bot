using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>转交谐振技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class PublishEraResonanceDraftCommandHandler : ICommandHandler<PublishEraResonanceDraftCommand,PublishEraResonanceDraftResponse>
{
    private readonly IEraResonanceService _service;

    public PublishEraResonanceDraftCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<PublishEraResonanceDraftResponse> Handle(IReceiveContext<PublishEraResonanceDraftCommand> context, CancellationToken cancellationToken)
    {
        return await _service.PublishDraftAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
