using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Email;
using Void.Bot.Message.Commands.Email;

namespace Void.Bot.Core.Handler.CommandHandler.Email;

public sealed class AuthorizeEmailCommandHandler(IEmailService service)
    : ICommandHandler<AuthorizeEmailCommand, AuthorizeEmailResponse>
{
    public async Task<AuthorizeEmailResponse> Handle(
        IReceiveContext<AuthorizeEmailCommand> context,
        CancellationToken cancellationToken)
    {
        return await service.AuthorizeAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
    }
}
