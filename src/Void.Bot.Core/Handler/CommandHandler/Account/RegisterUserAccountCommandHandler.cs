using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Events.EventAction;

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
        var response = await _service.RegisterUserAccountAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
        if (response.Data != null)
        {
            await context.PublishAsync(
                new AccountRegisteredEvent(response.Data.Id), cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}
