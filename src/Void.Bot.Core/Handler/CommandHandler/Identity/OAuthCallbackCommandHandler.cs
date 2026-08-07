using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public sealed class OAuthCallbackCommandHandler
    : ICommandHandler<OAuthCallbackCommand, OAuthCallbackResponse>
{
    private readonly IOAuthService _service;

    public OAuthCallbackCommandHandler(IOAuthService service)
    {
        _service = service;
    }

    public async Task<OAuthCallbackResponse> Handle(
        IReceiveContext<OAuthCallbackCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.CallbackAsync(
            context.Message.Platform,
            new OAuthCallbackContext(
                context.Message.Code,
                context.Message.State,
                context.Message.Error),
            cancellationToken).ConfigureAwait(false);
    }
}
