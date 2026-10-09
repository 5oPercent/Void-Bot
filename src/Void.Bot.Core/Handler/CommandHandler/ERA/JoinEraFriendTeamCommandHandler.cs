using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class JoinEraFriendTeamCommandHandler : ICommandHandler<JoinEraFriendTeamCommand, JoinEraFriendTeamResponse>
{
    private readonly IEraSocialService _service;

    public JoinEraFriendTeamCommandHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<JoinEraFriendTeamResponse> Handle(IReceiveContext<JoinEraFriendTeamCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.JoinEraFriendTeamAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
