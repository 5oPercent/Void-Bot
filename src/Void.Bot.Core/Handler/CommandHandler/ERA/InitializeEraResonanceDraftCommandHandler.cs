using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>谐振技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class InitializeEraResonanceDraftCommandHandler : ICommandHandler<InitializeEraResonanceDraftCommand, InitializeEraResonanceDraftResponse>
{
    private readonly IEraResonanceService _service;

    public InitializeEraResonanceDraftCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<InitializeEraResonanceDraftResponse> Handle(IReceiveContext<InitializeEraResonanceDraftCommand> context, CancellationToken cancellationToken)
    {
        return await _service.InitializeDraftAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
