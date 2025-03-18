using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public class DeleteApiKeyCommandHandler : ICommandHandler<DeleteApiKeyCommand, DeleteApiKeyResponse>
{
    private readonly IApiKeyService _service;

    public DeleteApiKeyCommandHandler(IApiKeyService service)
    {
        _service = service;
    }
    
    public async Task<DeleteApiKeyResponse> Handle(IReceiveContext<DeleteApiKeyCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteApiKeyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}