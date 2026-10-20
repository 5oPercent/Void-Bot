using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class PrepareEraPunchCommandHandler : ICommandHandler<PrepareEraPunchCommand, PrepareEraPunchResponse>
{
    private readonly IEraTeamService _service;

    public PrepareEraPunchCommandHandler(IEraTeamService service)
    {
        _service = service;
    }

    public async Task<PrepareEraPunchResponse> Handle(
        IReceiveContext<PrepareEraPunchCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.PreparePunchAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
