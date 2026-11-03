using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>敌人配置草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class SaveEraDraftEnemyCommandHandler : ICommandHandler<SaveEraDraftEnemyCommand, SaveEraDraftEnemyResponse>
{
    private readonly IEraResonanceService _service;

    public SaveEraDraftEnemyCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<SaveEraDraftEnemyResponse> Handle(IReceiveContext<SaveEraDraftEnemyCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveDraftEnemyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
