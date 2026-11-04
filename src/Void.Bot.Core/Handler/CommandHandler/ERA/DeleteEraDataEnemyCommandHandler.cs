using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Enemy;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>敌人配置消息转交业务服务，事务沿用工作单元。</summary>
public sealed class DeleteEraDataEnemyCommandHandler : ICommandHandler<DeleteEraDataEnemyCommand, DeleteEraDataEnemyResponse>
{
    private readonly IEraEnemyService _service;

    public DeleteEraDataEnemyCommandHandler(IEraEnemyService service)
    {
        _service = service;
    }

    public async Task<DeleteEraDataEnemyResponse> Handle(IReceiveContext<DeleteEraDataEnemyCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteDataEnemyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
