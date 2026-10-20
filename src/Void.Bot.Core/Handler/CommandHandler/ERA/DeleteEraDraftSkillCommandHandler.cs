using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Skill;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class DeleteEraDraftSkillCommandHandler : ICommandHandler<DeleteEraDraftSkillCommand, DeleteEraDraftSkillResponse>
{
    private readonly IEraSkillService _service;

    public DeleteEraDraftSkillCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<DeleteEraDraftSkillResponse> Handle(IReceiveContext<DeleteEraDraftSkillCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteDraftSkillAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
