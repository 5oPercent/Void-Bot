using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public class UnlinkUserAccountFromThirdPartyCommandHandler
    : ICommandHandler<UnlinkUserAccountFromThirdPartyCommand, UnlinkUserAccountFromThirdPartyResponse>
{
    private readonly IAccountService _service;

    public UnlinkUserAccountFromThirdPartyCommandHandler(IAccountService service)
    {
        _service = service;
    }

    public async Task<UnlinkUserAccountFromThirdPartyResponse> Handle(
        IReceiveContext<UnlinkUserAccountFromThirdPartyCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.UnlinkAccountFromThirdPartyAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
