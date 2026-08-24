using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Email;
using Void.Bot.Message.Commands.Email;

namespace Void.Bot.Core.Handler.CommandHandler.Email;

public sealed class RefreshEmailCredentialCommandHandler(IEmailService service)
    : ICommandHandler<RefreshEmailCredentialCommand, RefreshEmailCredentialResponse>
{
    public async Task<RefreshEmailCredentialResponse> Handle(
        IReceiveContext<RefreshEmailCredentialCommand> context,
        CancellationToken cancellationToken)
    {
        return await service.RefreshCredentialAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
    }
}
