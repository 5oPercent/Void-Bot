using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class LeaveEraTeamCommandHandler : ICommandHandler<LeaveEraTeamCommand, LeaveEraTeamResponse>
{
    private readonly IEraTeamService _service;

    public LeaveEraTeamCommandHandler(IEraTeamService service)
    {
        _service = service;
    }

    public async Task<LeaveEraTeamResponse> Handle(
        IReceiveContext<LeaveEraTeamCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.LeaveAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
