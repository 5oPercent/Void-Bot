using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetCurrentEraTeamRequestHandler : IRequestHandler<GetCurrentEraTeamRequest, GetCurrentEraTeamResponse>
{
    private readonly IEraTeamService _service;

    public GetCurrentEraTeamRequestHandler(IEraTeamService service)
    {
        _service = service;
    }

    public async Task<GetCurrentEraTeamResponse> Handle(
        IReceiveContext<GetCurrentEraTeamRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetCurrentAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
