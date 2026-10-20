using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class SetEraHostReadinessCommandHandler : ICommandHandler<SetEraHostReadinessCommand, SetEraHostReadinessResponse>
{
    private readonly IEraTeamService _service;

    public SetEraHostReadinessCommandHandler(IEraTeamService service)
    {
        _service = service;
    }

    public async Task<SetEraHostReadinessResponse> Handle(
        IReceiveContext<SetEraHostReadinessCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.SetHostReadinessAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
