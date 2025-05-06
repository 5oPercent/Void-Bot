using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestinyBaseProfileRequestHandler : IRequestHandler<GetDestiny2BaseProfileRequest, GetDestiny2BaseProfileResponse>
{
    private readonly IBungieService _service;

    public GetDestinyBaseProfileRequestHandler(IBungieService service)
    {
        _service = service;
    }
    
    public async Task<GetDestiny2BaseProfileResponse> Handle(IReceiveContext<GetDestiny2BaseProfileRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestiny2BaseProfileAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}