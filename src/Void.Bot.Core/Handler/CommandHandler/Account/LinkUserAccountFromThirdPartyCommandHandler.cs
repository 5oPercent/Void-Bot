using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public class LinkUserAccountFromThirdPartyCommandHandler : ICommandHandler<LinkUserAccountFromThirdPartyCommand, LinkUserAccountFromThirdPartyResponse>
{
    private readonly IAccountService _service;

    public LinkUserAccountFromThirdPartyCommandHandler(IAccountService service)
    {
        _service = service;
    }

    public async Task<LinkUserAccountFromThirdPartyResponse> Handle(IReceiveContext<LinkUserAccountFromThirdPartyCommand> context, CancellationToken cancellationToken)
    {
        return await _service.LinkAccountFromThirdPartyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}