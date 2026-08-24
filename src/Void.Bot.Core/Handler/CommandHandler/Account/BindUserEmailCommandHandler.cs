using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class BindUserEmailCommandHandler(IAccountService service)
    : ICommandHandler<BindUserEmailCommand, BindUserEmailResponse>
{
    public Task<BindUserEmailResponse> Handle(
        IReceiveContext<BindUserEmailCommand> context,
        CancellationToken cancellationToken)
    {
        return service.BindUserEmailAsync(context.Message, cancellationToken);
    }
}
