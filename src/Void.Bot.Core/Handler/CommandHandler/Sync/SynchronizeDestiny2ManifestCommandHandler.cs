using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Sync;
using Void.Bot.Message.Commands.Sync;

namespace Void.Bot.Core.Handler.CommandHandler.Sync;

public class SynchronizeDestiny2ManifestCommandHandler : ICommandHandler<SynchronizeDestiny2ManifestCommand, SynchronizeDestiny2ManifestResponse>
{
    private ISyncService _service;
    
    public SynchronizeDestiny2ManifestCommandHandler(ISyncService service)
    {
        _service = service;
    }
    
    public async Task<SynchronizeDestiny2ManifestResponse> Handle(IReceiveContext<SynchronizeDestiny2ManifestCommand> context, CancellationToken cancellationToken)
    {
        return await _service.SynchronizeDestiny2ManifestAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}