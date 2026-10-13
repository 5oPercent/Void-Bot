using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class RestoreEraDraftBuffCommandHandler : ICommandHandler<RestoreEraDraftBuffCommand, RestoreEraDraftBuffResponse>
{
    private readonly IEraSkillService _service;

    public RestoreEraDraftBuffCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<RestoreEraDraftBuffResponse> Handle(IReceiveContext<RestoreEraDraftBuffCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RestoreDraftBuffAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
