using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class WithdrawEraTeamInvitationCommandHandler : ICommandHandler<WithdrawEraTeamInvitationCommand, WithdrawEraTeamInvitationResponse>
{
    private readonly IEraSocialService _service;

    public WithdrawEraTeamInvitationCommandHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<WithdrawEraTeamInvitationResponse> Handle(IReceiveContext<WithdrawEraTeamInvitationCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.WithdrawEraTeamInvitationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
