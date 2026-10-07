using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class CreateEraTeamCommandHandler : ICommandHandler<CreateEraTeamCommand, CreateEraTeamResponse>
{
    private readonly IEraTeamService _service;

    public CreateEraTeamCommandHandler(IEraTeamService service)
    {
        _service = service;
    }

    public async Task<CreateEraTeamResponse> Handle(
        IReceiveContext<CreateEraTeamCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.CreateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
