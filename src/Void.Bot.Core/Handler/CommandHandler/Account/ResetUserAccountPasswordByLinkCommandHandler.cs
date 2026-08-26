using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;
namespace Void.Bot.Core.Handler.CommandHandler.Account;
public sealed class ResetUserAccountPasswordByLinkCommandHandler(IAccountService service)
    : ICommandHandler<ResetUserAccountPasswordByLinkCommand, ResetUserAccountPasswordByLinkResponse>
{
    public Task<ResetUserAccountPasswordByLinkResponse> Handle(
        IReceiveContext<ResetUserAccountPasswordByLinkCommand> context,
        CancellationToken cancellationToken) =>
        service.ResetUserAccountPasswordByLinkAsync(context.Message, cancellationToken);
}
