using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Sync;
using Void.Bot.Message.Request.Sync;

namespace Void.Bot.Core.Handler.RequestHandler.Sync;

public class GetDestiny2ManifestSyncOptionsRequestHandler(ISyncService service)
    : IRequestHandler<GetDestiny2ManifestSyncOptionsRequest, GetDestiny2ManifestSyncOptionsResponse>
{
    public Task<GetDestiny2ManifestSyncOptionsResponse> Handle(
        IReceiveContext<GetDestiny2ManifestSyncOptionsRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetDestiny2ManifestSyncOptionsAsync(cancellationToken);
    }
}
