using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Core.Handler.CommandHandler.Identity;

public class LoginBungieCommandHandler : ICommandHandler<LoginBungieCommand, LoginBungieResponse>
{
    private readonly IIdentityService _service;

    public LoginBungieCommandHandler(IIdentityService service)
    {
        _service = service;
    }
    
    public async Task<LoginBungieResponse> Handle(IReceiveContext<LoginBungieCommand> context, CancellationToken cancellationToken)
    {
        return await _service.LinkToBungieAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}