using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class CompleteEraHostMigrationCommandHandler : ICommandHandler<CompleteEraHostMigrationCommand, CompleteEraHostMigrationResponse>
{
    private readonly IEraTeamService _service;

    public CompleteEraHostMigrationCommandHandler(IEraTeamService service)
    {
        _service = service;
    }

    public async Task<CompleteEraHostMigrationResponse> Handle(
        IReceiveContext<CompleteEraHostMigrationCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.CompleteHostMigrationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
