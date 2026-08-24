using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class ResetUserAccountPasswordCommandHandler(IAccountService service)
    : ICommandHandler<ResetUserAccountPasswordCommand, ResetUserAccountPasswordResponse>
{
    public Task<ResetUserAccountPasswordResponse> Handle(
        IReceiveContext<ResetUserAccountPasswordCommand> context,
        CancellationToken cancellationToken)
    {
        return service.ResetUserAccountPasswordAsync(context.Message, cancellationToken);
    }
}
