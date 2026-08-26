using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;
namespace Void.Bot.Core.Handler.CommandHandler.Account;
public sealed class SendUserAccountPasswordResetLinkCommandHandler(IAccountService service)
    : ICommandHandler<SendUserAccountPasswordResetLinkCommand, SendUserAccountPasswordResetLinkResponse>
{
    public Task<SendUserAccountPasswordResetLinkResponse> Handle(
        IReceiveContext<SendUserAccountPasswordResetLinkCommand> context,
        CancellationToken cancellationToken) =>
        service.SendUserAccountPasswordResetLinkAsync(context.Message, cancellationToken);
}
