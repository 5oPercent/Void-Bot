using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Social;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class RejectEraTeamInvitationCommandHandler : ICommandHandler<RejectEraTeamInvitationCommand, RejectEraTeamInvitationResponse>
{
    private readonly IEraSocialService _service;

    public RejectEraTeamInvitationCommandHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<RejectEraTeamInvitationResponse> Handle(IReceiveContext<RejectEraTeamInvitationCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.RejectEraTeamInvitationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
