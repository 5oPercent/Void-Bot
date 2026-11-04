using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.AbilityProfile;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class GrantEraLearnedResonanceCommandHandler : ICommandHandler<GrantEraLearnedResonanceCommand, GrantEraLearnedResonanceResponse>
{
    private readonly IEraAbilityProfileService _service;

    public GrantEraLearnedResonanceCommandHandler(IEraAbilityProfileService service)
    {
        _service = service;
    }

    public async Task<GrantEraLearnedResonanceResponse> Handle(
        IReceiveContext<GrantEraLearnedResonanceCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GrantResonanceAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
