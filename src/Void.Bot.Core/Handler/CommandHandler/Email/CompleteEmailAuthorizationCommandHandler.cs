using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Email;
using Void.Bot.Message.Commands.Email;

namespace Void.Bot.Core.Handler.CommandHandler.Email;

public sealed class CompleteEmailAuthorizationCommandHandler(IEmailService service)
    : ICommandHandler<CompleteEmailAuthorizationCommand, CompleteEmailAuthorizationResponse>
{
    public async Task<CompleteEmailAuthorizationResponse> Handle(
        IReceiveContext<CompleteEmailAuthorizationCommand> context,
        CancellationToken cancellationToken)
    {
        return await service.CompleteAuthorizationAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
    }
}
