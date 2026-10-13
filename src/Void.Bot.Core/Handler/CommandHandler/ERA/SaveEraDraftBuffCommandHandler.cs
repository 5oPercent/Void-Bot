using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class SaveEraDraftBuffCommandHandler : ICommandHandler<SaveEraDraftBuffCommand, SaveEraDraftBuffResponse>
{
    private readonly IEraSkillService _service;

    public SaveEraDraftBuffCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<SaveEraDraftBuffResponse> Handle(IReceiveContext<SaveEraDraftBuffCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveDraftBuffAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
