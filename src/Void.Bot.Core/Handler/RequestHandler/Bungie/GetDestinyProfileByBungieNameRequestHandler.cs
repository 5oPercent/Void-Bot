using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestinyProfileByBungieNameRequestHandler : IRequestHandler<GetDestiny2ProfileListByBungieNameRequest, GetDestiny2ProfileListByBungieNameResponse>
{
    private readonly IBungieService _service;

    public GetDestinyProfileByBungieNameRequestHandler(IBungieService service)
    {
        _service = service;
    }
    
    public async Task<GetDestiny2ProfileListByBungieNameResponse> Handle(IReceiveContext<GetDestiny2ProfileListByBungieNameRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestiny2ProfileListByBungieNameAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}