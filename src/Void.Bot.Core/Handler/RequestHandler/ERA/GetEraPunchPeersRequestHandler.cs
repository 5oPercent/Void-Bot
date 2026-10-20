using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraPunchPeersRequestHandler : IRequestHandler<GetEraPunchPeersRequest, GetEraPunchPeersResponse>
{
    private readonly IEraTeamService _service;

    public GetEraPunchPeersRequestHandler(IEraTeamService service)
    {
        _service = service;
    }

    public async Task<GetEraPunchPeersResponse> Handle(
        IReceiveContext<GetEraPunchPeersRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetPeersAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
