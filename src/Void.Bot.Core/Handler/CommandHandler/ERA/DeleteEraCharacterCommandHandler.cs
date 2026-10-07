using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class DeleteEraCharacterCommandHandler : ICommandHandler<DeleteEraCharacterCommand, DeleteEraCharacterResponse>
{
    private readonly IEraService _service;

    public DeleteEraCharacterCommandHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<DeleteEraCharacterResponse> Handle(
        IReceiveContext<DeleteEraCharacterCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .DeleteCharacterAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}