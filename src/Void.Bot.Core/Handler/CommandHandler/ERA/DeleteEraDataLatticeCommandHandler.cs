using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Lattice;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>晶格配置消息转交本类别业务服务，事务沿用工作单元。</summary>
public sealed class DeleteEraDataLatticeCommandHandler : ICommandHandler<DeleteEraDataLatticeCommand, DeleteEraDataLatticeResponse>
{
    private readonly IEraLatticeService _service;

    public DeleteEraDataLatticeCommandHandler(IEraLatticeService service)
    {
        _service = service;
    }

    public async Task<DeleteEraDataLatticeResponse> Handle(IReceiveContext<DeleteEraDataLatticeCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteDataLatticeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
