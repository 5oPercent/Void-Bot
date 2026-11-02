using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>谐振技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class RestoreEraDraftResonanceCommandHandler : ICommandHandler<RestoreEraDraftResonanceCommand, RestoreEraDraftResonanceResponse>
{
    private readonly IEraResonanceService _service;

    public RestoreEraDraftResonanceCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<RestoreEraDraftResonanceResponse> Handle(IReceiveContext<RestoreEraDraftResonanceCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RestoreDraftResonanceAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
