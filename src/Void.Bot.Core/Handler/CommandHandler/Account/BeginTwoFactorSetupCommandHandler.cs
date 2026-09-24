using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public sealed class BeginTwoFactorSetupCommandHandler(IAccountTwoFactorService service)
    : ICommandHandler<BeginTwoFactorSetupCommand, BeginTwoFactorSetupResponse>
{
    public Task<BeginTwoFactorSetupResponse> Handle(
        IReceiveContext<BeginTwoFactorSetupCommand> context,
        CancellationToken cancellationToken)
    {
        return service.BeginSetupAsync(context.Message, cancellationToken);
    }
}
