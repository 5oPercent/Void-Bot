using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraTeamRequestHandler : IRequestHandler<GetEraTeamRequest, GetEraTeamResponse>
{
    private readonly IEraTeamService _service;

    public GetEraTeamRequestHandler(IEraTeamService service)
    {
        _service = service;
    }

    public async Task<GetEraTeamResponse> Handle(
        IReceiveContext<GetEraTeamRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
