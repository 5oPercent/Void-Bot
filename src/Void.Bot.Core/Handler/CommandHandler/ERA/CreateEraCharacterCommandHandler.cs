using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class CreateEraCharacterCommandHandler : ICommandHandler<CreateEraCharacterCommand, CreateEraCharacterResponse>
{
    private readonly IEraService _service;

    public CreateEraCharacterCommandHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<CreateEraCharacterResponse> Handle(
        IReceiveContext<CreateEraCharacterCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .CreateCharacterAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
