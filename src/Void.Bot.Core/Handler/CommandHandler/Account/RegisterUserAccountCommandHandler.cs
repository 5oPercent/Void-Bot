using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Handler.CommandHandler.Account;

public class RegisterUserAccountCommandHandler : ICommandHandler<RegisterUserAccountCommand, RegisterUserAccountResponse>
{
    private readonly IAccountService _service;

    public RegisterUserAccountCommandHandler(IAccountService service)
    {
        _service = service;
    }
    
    public async Task<RegisterUserAccountResponse> Handle(IReceiveContext<RegisterUserAccountCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RegisterUserAccountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}