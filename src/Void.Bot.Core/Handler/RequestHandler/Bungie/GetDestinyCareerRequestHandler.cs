using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Request.Bungie;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestinyCareerRequestHandler : IRequestHandler<GetDestinyCareerRequest, GetDestinyCareerResponse>
{
    private IBungieService _service;

    public GetDestinyCareerRequestHandler(IBungieService service)
    {
        _service = service;
    }
    
    public async Task<GetDestinyCareerResponse> Handle(IReceiveContext<GetDestinyCareerRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestinyCareerAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}