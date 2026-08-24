using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class SendPasswordResetCodeCommandHandler(IAccountService service)
    : ICommandHandler<SendPasswordResetCodeCommand, SendPasswordResetCodeResponse>
{
    public Task<SendPasswordResetCodeResponse> Handle(
        IReceiveContext<SendPasswordResetCodeCommand> context,
        CancellationToken cancellationToken)
    {
        return service.SendPasswordResetCodeAsync(context.Message, cancellationToken);
    }
}
