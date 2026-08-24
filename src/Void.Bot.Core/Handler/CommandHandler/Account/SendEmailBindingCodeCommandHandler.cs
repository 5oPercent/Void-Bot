using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class SendEmailBindingCodeCommandHandler(IAccountService service)
    : ICommandHandler<SendEmailBindingCodeCommand, SendEmailBindingCodeResponse>
{
    public Task<SendEmailBindingCodeResponse> Handle(
        IReceiveContext<SendEmailBindingCodeCommand> context,
        CancellationToken cancellationToken)
    {
        return service.SendEmailBindingCodeAsync(context.Message, cancellationToken);
    }
}
