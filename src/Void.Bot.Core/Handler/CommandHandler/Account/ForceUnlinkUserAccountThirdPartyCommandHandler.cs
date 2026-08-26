using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class ForceUnlinkUserAccountThirdPartyCommandHandler(IAccountService service)
    : ICommandHandler<ForceUnlinkUserAccountThirdPartyCommand, ForceUnlinkUserAccountThirdPartyResponse>
{
    public async Task<ForceUnlinkUserAccountThirdPartyResponse> Handle(
        IReceiveContext<ForceUnlinkUserAccountThirdPartyCommand> context,
        CancellationToken cancellationToken)
    {
        var response = await service.ForceUnlinkUserAccountThirdPartyAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
        if (response.Code == default)
        {
            await context.PublishAsync(
                new AccountThirdPartyForceUnlinkedEvent(
                    context.Message.UserId,
                    context.Message.Platform),
                cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}
