using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Skill;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class RestoreEraDraftSystemCommandHandler : ICommandHandler<RestoreEraDraftSystemCommand, RestoreEraDraftSystemResponse>
{
    private readonly IEraSkillService _service;

    public RestoreEraDraftSystemCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<RestoreEraDraftSystemResponse> Handle(IReceiveContext<RestoreEraDraftSystemCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RestoreDraftSystemAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
