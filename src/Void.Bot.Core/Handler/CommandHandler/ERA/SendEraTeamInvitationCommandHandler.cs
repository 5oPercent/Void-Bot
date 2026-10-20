using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Social;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class SendEraTeamInvitationCommandHandler : ICommandHandler<SendEraTeamInvitationCommand, SendEraTeamInvitationResponse>
{
    private readonly IEraSocialService _service;

    public SendEraTeamInvitationCommandHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<SendEraTeamInvitationResponse> Handle(IReceiveContext<SendEraTeamInvitationCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.SendEraTeamInvitationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
