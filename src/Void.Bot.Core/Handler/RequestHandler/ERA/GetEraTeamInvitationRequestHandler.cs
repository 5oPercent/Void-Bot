using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Social;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraTeamInvitationRequestHandler : IRequestHandler<GetEraTeamInvitationRequest, GetEraTeamInvitationResponse>
{
    private readonly IEraSocialService _service;

    public GetEraTeamInvitationRequestHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<GetEraTeamInvitationResponse> Handle(IReceiveContext<GetEraTeamInvitationRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetEraTeamInvitationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
