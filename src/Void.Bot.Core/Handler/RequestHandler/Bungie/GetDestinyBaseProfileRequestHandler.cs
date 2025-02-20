using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Request.Bungie;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestinyBaseProfileRequestHandler : IRequestHandler<GetDestinyBaseProfileRequest, GetDestinyBaseProfileResponse>
{
    private readonly IBungieService _service;

    public GetDestinyBaseProfileRequestHandler(IBungieService service)
    {
        _service = service;
    }
    
    public async Task<GetDestinyBaseProfileResponse> Handle(IReceiveContext<GetDestinyBaseProfileRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestinyBaseProfileAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}