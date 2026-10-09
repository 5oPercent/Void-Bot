using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class AcceptEraTeamInvitationCommandHandler : ICommandHandler<AcceptEraTeamInvitationCommand, AcceptEraTeamInvitationResponse>
{
    private readonly IEraSocialService _service;

    public AcceptEraTeamInvitationCommandHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<AcceptEraTeamInvitationResponse> Handle(IReceiveContext<AcceptEraTeamInvitationCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.AcceptEraTeamInvitationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
