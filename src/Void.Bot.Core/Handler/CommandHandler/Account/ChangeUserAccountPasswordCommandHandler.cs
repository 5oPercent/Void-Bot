using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public class ChangeUserAccountPasswordCommandHandler
    : ICommandHandler<ChangeUserAccountPasswordCommand, ChangeUserAccountPasswordResponse>
{
    private readonly IAccountService _service;

    public ChangeUserAccountPasswordCommandHandler(IAccountService service)
    {
        _service = service;
    }

    public async Task<ChangeUserAccountPasswordResponse> Handle(
        IReceiveContext<ChangeUserAccountPasswordCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.ChangeUserAccountPasswordAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
    }
}
