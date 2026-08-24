using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class SendRegistrationEmailCodeCommandHandler(IAccountService service)
    : ICommandHandler<SendRegistrationEmailCodeCommand, SendRegistrationEmailCodeResponse>
{
    public Task<SendRegistrationEmailCodeResponse> Handle(
        IReceiveContext<SendRegistrationEmailCodeCommand> context,
        CancellationToken cancellationToken)
    {
        return service.SendRegistrationEmailCodeAsync(context.Message, cancellationToken);
    }
}
