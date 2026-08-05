using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public sealed class OAuthAuthorizeCommandHandler
    : ICommandHandler<OAuthAuthorizeCommand, OAuthAuthorizeResponse>
{
    private readonly IOAuthService _service;

    public OAuthAuthorizeCommandHandler(IOAuthService service)
    {
        _service = service;
    }

    public async Task<OAuthAuthorizeResponse> Handle(
        IReceiveContext<OAuthAuthorizeCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.AuthorizeAsync(
            context.Message.Platform,
            cancellationToken).ConfigureAwait(false);
    }
}
