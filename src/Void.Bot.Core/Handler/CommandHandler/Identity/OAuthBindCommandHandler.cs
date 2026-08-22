using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public sealed class OAuthBindCommandHandler
    : ICommandHandler<OAuthBindCommand, OAuthBindResponse>
{
    private readonly IOAuthService _service;

    public OAuthBindCommandHandler(IOAuthService service)
    {
        _service = service;
    }

    public async Task<OAuthBindResponse> Handle(
        IReceiveContext<OAuthBindCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.BindAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
