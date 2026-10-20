using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class HeartbeatEraTeamCommandHandler : ICommandHandler<HeartbeatEraTeamCommand, HeartbeatEraTeamResponse>
{
    private readonly IEraTeamService _service;

    public HeartbeatEraTeamCommandHandler(IEraTeamService service)
    {
        _service = service;
    }

    public async Task<HeartbeatEraTeamResponse> Handle(
        IReceiveContext<HeartbeatEraTeamCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.HeartbeatAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
