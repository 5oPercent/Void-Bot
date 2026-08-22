using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public sealed class OAuthRegisterCommandHandler
    : ICommandHandler<OAuthRegisterCommand, OAuthRegisterResponse>
{
    private readonly IOAuthService _service;

    public OAuthRegisterCommandHandler(IOAuthService service)
    {
        _service = service;
    }

    public async Task<OAuthRegisterResponse> Handle(
        IReceiveContext<OAuthRegisterCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.RegisterAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
