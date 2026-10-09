using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class LookupEraTeamRequestHandler : IRequestHandler<LookupEraTeamRequest, LookupEraTeamResponse>
{
    private readonly IEraTeamService _service;
    public LookupEraTeamRequestHandler(IEraTeamService service)
    {
        _service = service;
    }

    public Task<LookupEraTeamResponse> Handle(IReceiveContext<LookupEraTeamRequest> context, CancellationToken cancellationToken)
    {
        return _service.LookupAsync(context.Message, cancellationToken);
    }
}
