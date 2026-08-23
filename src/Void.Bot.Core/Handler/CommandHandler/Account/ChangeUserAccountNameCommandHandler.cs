using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public class ChangeUserAccountNameCommandHandler
    : ICommandHandler<ChangeUserAccountNameCommand, ChangeUserAccountNameResponse>
{
    private readonly IAccountService _service;

    public ChangeUserAccountNameCommandHandler(IAccountService service)
    {
        _service = service;
    }

    public async Task<ChangeUserAccountNameResponse> Handle(
        IReceiveContext<ChangeUserAccountNameCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.ChangeUserAccountNameAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
    }
}
