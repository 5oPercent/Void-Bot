using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>转交谐振技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class ImportEraResonanceDraftCommandHandler : ICommandHandler<ImportEraResonanceDraftCommand,ImportEraResonanceDraftResponse>
{
    private readonly IEraResonanceService _service;

    public ImportEraResonanceDraftCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<ImportEraResonanceDraftResponse> Handle(IReceiveContext<ImportEraResonanceDraftCommand> context, CancellationToken cancellationToken)
    {
        return await _service.ImportDraftAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
