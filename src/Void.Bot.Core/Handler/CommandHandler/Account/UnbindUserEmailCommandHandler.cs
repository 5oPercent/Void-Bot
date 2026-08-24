using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class UnbindUserEmailCommandHandler(IAccountService service)
    : ICommandHandler<UnbindUserEmailCommand, UnbindUserEmailResponse>
{
    public Task<UnbindUserEmailResponse> Handle(
        IReceiveContext<UnbindUserEmailCommand> context,
        CancellationToken cancellationToken)
    {
        return service.UnbindUserEmailAsync(context.Message, cancellationToken);
    }
}
