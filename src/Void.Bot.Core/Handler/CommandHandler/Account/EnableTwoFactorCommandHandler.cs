using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class EnableTwoFactorCommandHandler(IAccountTwoFactorService service)
    : ICommandHandler<EnableTwoFactorCommand, EnableTwoFactorResponse>
{
    public Task<EnableTwoFactorResponse> Handle(
        IReceiveContext<EnableTwoFactorCommand> context,
        CancellationToken cancellationToken)
    {
        return service.EnableAsync(context.Message, cancellationToken);
    }
}
