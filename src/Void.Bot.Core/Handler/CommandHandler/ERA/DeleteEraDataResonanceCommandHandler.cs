using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>谐振技能配置消息转交业务服务，事务沿用工作单元。</summary>
public sealed class DeleteEraDataResonanceCommandHandler : ICommandHandler<DeleteEraDataResonanceCommand, DeleteEraDataResonanceResponse>
{
    private readonly IEraResonanceService _service;

    public DeleteEraDataResonanceCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<DeleteEraDataResonanceResponse> Handle(IReceiveContext<DeleteEraDataResonanceCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteDataResonanceAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
