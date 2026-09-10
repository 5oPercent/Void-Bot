using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class RegenerateTwoFactorRecoveryCodesCommandHandler(IAccountTwoFactorService service)
    : ICommandHandler<RegenerateTwoFactorRecoveryCodesCommand, RegenerateTwoFactorRecoveryCodesResponse>
{
    public Task<RegenerateTwoFactorRecoveryCodesResponse> Handle(
        IReceiveContext<RegenerateTwoFactorRecoveryCodesCommand> context,
        CancellationToken cancellationToken)
    {
        return service.RegenerateRecoveryCodesAsync(context.Message, cancellationToken);
    }
}
