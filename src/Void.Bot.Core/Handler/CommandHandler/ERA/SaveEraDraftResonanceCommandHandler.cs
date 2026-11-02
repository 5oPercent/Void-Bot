using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>谐振技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class SaveEraDraftResonanceCommandHandler : ICommandHandler<SaveEraDraftResonanceCommand, SaveEraDraftResonanceResponse>
{
    private readonly IEraResonanceService _service;

    public SaveEraDraftResonanceCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<SaveEraDraftResonanceResponse> Handle(IReceiveContext<SaveEraDraftResonanceCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveDraftResonanceAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
