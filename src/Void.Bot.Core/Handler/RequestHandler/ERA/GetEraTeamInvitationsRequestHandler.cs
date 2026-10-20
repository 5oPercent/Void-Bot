using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Social;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraTeamInvitationsRequestHandler : IRequestHandler<GetEraTeamInvitationsRequest, GetEraTeamInvitationsResponse>
{
    private readonly IEraSocialService _service;

    public GetEraTeamInvitationsRequestHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<GetEraTeamInvitationsResponse> Handle(IReceiveContext<GetEraTeamInvitationsRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetEraTeamInvitationsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
