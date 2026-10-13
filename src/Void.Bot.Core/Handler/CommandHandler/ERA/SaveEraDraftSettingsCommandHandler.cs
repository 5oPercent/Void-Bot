using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>技能草稿消息转交业务服务，事务沿用工作单元。</summary>
public sealed class SaveEraDraftSettingsCommandHandler : ICommandHandler<SaveEraDraftSettingsCommand, SaveEraDraftSettingsResponse>
{
    private readonly IEraSkillService _service;

    public SaveEraDraftSettingsCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<SaveEraDraftSettingsResponse> Handle(IReceiveContext<SaveEraDraftSettingsCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SaveDraftSettingsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
