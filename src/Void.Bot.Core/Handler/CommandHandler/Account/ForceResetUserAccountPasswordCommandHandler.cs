using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class ForceResetUserAccountPasswordCommandHandler(IAccountService service)
    : ICommandHandler<ForceResetUserAccountPasswordCommand, ForceResetUserAccountPasswordResponse>
{
    public async Task<ForceResetUserAccountPasswordResponse> Handle(
        IReceiveContext<ForceResetUserAccountPasswordCommand> context,
        CancellationToken cancellationToken)
    {
        var response = await service.ForceResetUserAccountPasswordAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
        if (response.Code == default)
        {
            await context.PublishAsync(
                new AccountPasswordForceResetEvent(
                    context.Message.UserId,
                    context.Message.NewPassword),
                cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}
