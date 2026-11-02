using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

/// <summary>转交谐振技能编辑／发布服务，沿用工作单元事务。</summary>
public sealed class RestoreEraDraftPublicationCommandHandler : ICommandHandler<RestoreEraDraftPublicationCommand,RestoreEraDraftPublicationResponse>
{
    private readonly IEraResonanceService _service;

    public RestoreEraDraftPublicationCommandHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<RestoreEraDraftPublicationResponse> Handle(IReceiveContext<RestoreEraDraftPublicationCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RestorePublicationToDraftAsync(context.Message,cancellationToken).ConfigureAwait(false);
    }
}
