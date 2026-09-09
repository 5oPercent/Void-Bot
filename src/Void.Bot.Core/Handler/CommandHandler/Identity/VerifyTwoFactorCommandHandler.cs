using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public sealed class VerifyTwoFactorCommandHandler(IIdentityService service)
    : ICommandHandler<VerifyTwoFactorCommand, VerifyTwoFactorResponse>
{
    public Task<VerifyTwoFactorResponse> Handle(
        IReceiveContext<VerifyTwoFactorCommand> context,
        CancellationToken cancellationToken)
    {
        return service.VerifyTwoFactorAsync(context.Message, cancellationToken);
    }
}
