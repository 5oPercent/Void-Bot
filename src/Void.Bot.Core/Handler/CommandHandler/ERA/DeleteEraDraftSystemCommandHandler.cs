using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class DeleteEraDraftSystemCommandHandler : ICommandHandler<DeleteEraDraftSystemCommand, DeleteEraDraftSystemResponse>
{
    private readonly IEraSkillService _service;

    public DeleteEraDraftSystemCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<DeleteEraDraftSystemResponse> Handle(IReceiveContext<DeleteEraDraftSystemCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteDraftSystemAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
