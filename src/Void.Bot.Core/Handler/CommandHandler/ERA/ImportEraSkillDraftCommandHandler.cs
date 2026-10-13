using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>转交技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class ImportEraSkillDraftCommandHandler : ICommandHandler<ImportEraSkillDraftCommand,ImportEraSkillDraftResponse>
{
    private readonly IEraSkillService _service;

    public ImportEraSkillDraftCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<ImportEraSkillDraftResponse> Handle(IReceiveContext<ImportEraSkillDraftCommand> context, CancellationToken cancellationToken)
    {
        return await _service.ImportDraftAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
