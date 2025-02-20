using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Request.Bungie;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestinyProfileByBungieNameRequestHandler : IRequestHandler<GetDestinyProfileListByBungieNameRequest, GetDestinyProfileListByBungieNameResponse>
{
    private readonly IBungieService _service;

    public GetDestinyProfileByBungieNameRequestHandler(IBungieService service)
    {
        _service = service;
    }
    
    public async Task<GetDestinyProfileListByBungieNameResponse> Handle(IReceiveContext<GetDestinyProfileListByBungieNameRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestinyProfileListByBungieNameAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}