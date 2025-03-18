using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public class CreateApiKeyCommandHandler : ICommandHandler<CreateApiKeyCommand, CreateApiKeyResponse>
{
    private readonly IApiKeyService _service;

    public CreateApiKeyCommandHandler(IApiKeyService service)
    {
        _service = service;
    }
    
    public async Task<CreateApiKeyResponse> Handle(IReceiveContext<CreateApiKeyCommand> context, CancellationToken cancellationToken)
    {
        return await _service.CreateApiKeyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}