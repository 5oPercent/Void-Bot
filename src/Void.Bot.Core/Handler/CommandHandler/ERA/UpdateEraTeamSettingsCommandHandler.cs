using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Social;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class UpdateEraTeamSettingsCommandHandler : ICommandHandler<UpdateEraTeamSettingsCommand, UpdateEraTeamSettingsResponse>
{
    private readonly IEraSocialService _service;

    public UpdateEraTeamSettingsCommandHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<UpdateEraTeamSettingsResponse> Handle(IReceiveContext<UpdateEraTeamSettingsCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.UpdateEraTeamSettingsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
