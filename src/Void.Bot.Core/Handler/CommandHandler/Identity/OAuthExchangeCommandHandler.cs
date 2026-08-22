using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public sealed class OAuthExchangeCommandHandler
    : ICommandHandler<OAuthExchangeCommand, OAuthExchangeResponse>
{
    private readonly IOAuthService _service;

    public OAuthExchangeCommandHandler(IOAuthService service)
    {
        _service = service;
    }

    public async Task<OAuthExchangeResponse> Handle(
        IReceiveContext<OAuthExchangeCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.ExchangeAsync(
            context.Message.Code,
            cancellationToken).ConfigureAwait(false);
    }
}
