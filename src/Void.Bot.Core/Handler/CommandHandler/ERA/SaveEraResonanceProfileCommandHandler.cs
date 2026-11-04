using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.AbilityProfile;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class SaveEraResonanceProfileCommandHandler : ICommandHandler<SaveEraResonanceProfileCommand, SaveEraResonanceProfileResponse>
{
    private readonly IEraAbilityProfileService _service;

    public SaveEraResonanceProfileCommandHandler(IEraAbilityProfileService service)
    {
        _service = service;
    }

    public async Task<SaveEraResonanceProfileResponse> Handle(
        IReceiveContext<SaveEraResonanceProfileCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .SaveProfileAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
