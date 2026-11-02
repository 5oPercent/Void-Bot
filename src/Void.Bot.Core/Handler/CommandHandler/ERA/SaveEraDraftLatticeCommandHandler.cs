using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>谐振技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class SaveEraDraftLatticeCommandHandler : ICommandHandler<SaveEraDraftLatticeCommand, SaveEraDraftLatticeResponse>
{
    private readonly IEraResonanceService _service;

    public SaveEraDraftLatticeCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<SaveEraDraftLatticeResponse> Handle(IReceiveContext<SaveEraDraftLatticeCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveDraftLatticeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
