using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Email;
using Void.Bot.Message.Commands.Email;

namespace Void.Bot.Core.Handler.CommandHandler.Email;

public sealed class SendTestEmailCommandHandler(IEmailService service)
    : ICommandHandler<SendTestEmailCommand, SendTestEmailResponse>
{
    public async Task<SendTestEmailResponse> Handle(
        IReceiveContext<SendTestEmailCommand> context,
        CancellationToken cancellationToken)
    {
        return await service.SendTestAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
    }
}
