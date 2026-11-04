using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Effect;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>效果配置消息转交本类别业务服务，事务沿用工作单元。</summary>
public sealed class SaveEraDataBuffCommandHandler : ICommandHandler<SaveEraDataBuffCommand, SaveEraDataBuffResponse>
{
    private readonly IEraEffectService _service;

    public SaveEraDataBuffCommandHandler(IEraEffectService service)
    {
        _service = service;
    }

    public async Task<SaveEraDataBuffResponse> Handle(IReceiveContext<SaveEraDataBuffCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveDataBuffAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
