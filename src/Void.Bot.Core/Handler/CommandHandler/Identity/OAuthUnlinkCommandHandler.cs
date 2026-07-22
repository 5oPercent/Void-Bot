using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public sealed class OAuthUnlinkCommandHandler
    : ICommandHandler<OAuthUnlinkCommand, OAuthUnlinkResponse>
{
    private readonly IOAuthService _service;

    public OAuthUnlinkCommandHandler(IOAuthService service)
    {
        _service = service;
    }

    public async Task<OAuthUnlinkResponse> Handle(
        IReceiveContext<OAuthUnlinkCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.UnlinkAsync(
            context.Message.Platform,
            cancellationToken).ConfigureAwait(false);
    }
}
