using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class DisableTwoFactorCommandHandler(IAccountTwoFactorService service)
    : ICommandHandler<DisableTwoFactorCommand, DisableTwoFactorResponse>
{
    public Task<DisableTwoFactorResponse> Handle(
        IReceiveContext<DisableTwoFactorCommand> context,
        CancellationToken cancellationToken)
    {
        return service.DisableAsync(context.Message, cancellationToken);
    }
}
