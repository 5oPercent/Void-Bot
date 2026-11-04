using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>谐振技能配置消息转交业务服务，事务沿用工作单元。</summary>
public sealed class SaveEraDataResonanceCommandHandler : ICommandHandler<SaveEraDataResonanceCommand, SaveEraDataResonanceResponse>
{
    private readonly IEraResonanceService _service;

    public SaveEraDataResonanceCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<SaveEraDataResonanceResponse> Handle(IReceiveContext<SaveEraDataResonanceCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveDataResonanceAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
