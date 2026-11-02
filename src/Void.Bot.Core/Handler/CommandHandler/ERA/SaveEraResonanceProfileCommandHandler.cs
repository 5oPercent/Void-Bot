using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class SaveEraResonanceProfileCommandHandler : ICommandHandler<SaveEraResonanceProfileCommand, SaveEraResonanceProfileResponse>
{
    private readonly IEraResonanceService _service;

    public SaveEraResonanceProfileCommandHandler(IEraResonanceService service)
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
