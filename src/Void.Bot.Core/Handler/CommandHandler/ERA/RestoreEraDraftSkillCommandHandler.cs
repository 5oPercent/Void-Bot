using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Skill;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class RestoreEraDraftSkillCommandHandler : ICommandHandler<RestoreEraDraftSkillCommand, RestoreEraDraftSkillResponse>
{
    private readonly IEraSkillService _service;

    public RestoreEraDraftSkillCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<RestoreEraDraftSkillResponse> Handle(IReceiveContext<RestoreEraDraftSkillCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RestoreDraftSkillAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
