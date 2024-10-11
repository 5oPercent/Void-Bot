using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public class LoginSteamCommandHandler : ICommandHandler<LoginSteamCommand, LoginSteamResponse>
{
    private readonly IIdentityService _service;

    public LoginSteamCommandHandler(IIdentityService service)
    {
        _service = service;
    }
    
    public async Task<LoginSteamResponse> Handle(IReceiveContext<LoginSteamCommand> context, CancellationToken cancellationToken)
    {
        return await _service.LinkToSteamAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}