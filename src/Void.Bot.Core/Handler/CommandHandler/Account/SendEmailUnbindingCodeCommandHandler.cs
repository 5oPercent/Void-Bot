using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class SendEmailUnbindingCodeCommandHandler(IAccountService service)
    : ICommandHandler<SendEmailUnbindingCodeCommand, SendEmailUnbindingCodeResponse>
{
    public Task<SendEmailUnbindingCodeResponse> Handle(
        IReceiveContext<SendEmailUnbindingCodeCommand> context,
        CancellationToken cancellationToken)
    {
        return service.SendEmailUnbindingCodeAsync(context.Message, cancellationToken);
    }
}
