using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.CombatRelation;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>种族阵营关系配置消息转交本类别业务服务，事务沿用工作单元。</summary>
public sealed class SaveEraDataSettingsCommandHandler : ICommandHandler<SaveEraDataSettingsCommand, SaveEraDataSettingsResponse>
{
    private readonly IEraCombatRelationService _service;

    public SaveEraDataSettingsCommandHandler(IEraCombatRelationService service)
    {
        _service = service;
    }

    public async Task<SaveEraDataSettingsResponse> Handle(IReceiveContext<SaveEraDataSettingsCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveDataSettingsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
