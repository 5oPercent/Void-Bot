using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class InitializeEraPlayerCommandHandler : ICommandHandler<InitializeEraPlayerCommand, InitializeEraPlayerResponse>
{
    private readonly IEraService _service;

    public InitializeEraPlayerCommandHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<InitializeEraPlayerResponse> Handle(
        IReceiveContext<InitializeEraPlayerCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .InitializePlayerAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
