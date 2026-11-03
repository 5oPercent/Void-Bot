using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>敌人配置草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class RestoreEraDraftEnemyCommandHandler : ICommandHandler<RestoreEraDraftEnemyCommand, RestoreEraDraftEnemyResponse>
{
    private readonly IEraResonanceService _service;

    public RestoreEraDraftEnemyCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<RestoreEraDraftEnemyResponse> Handle(IReceiveContext<RestoreEraDraftEnemyCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RestoreDraftEnemyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
