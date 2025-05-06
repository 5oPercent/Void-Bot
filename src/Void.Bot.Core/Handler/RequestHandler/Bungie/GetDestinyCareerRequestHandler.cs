using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestinyCareerRequestHandler : IRequestHandler<GetDestiny2CareerRequest, GetDestiny2CareerResponse>
{
    private IBungieService _service;

    public GetDestinyCareerRequestHandler(IBungieService service)
    {
        _service = service;
    }
    
    public async Task<GetDestiny2CareerResponse> Handle(IReceiveContext<GetDestiny2CareerRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestiny2CareerAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}