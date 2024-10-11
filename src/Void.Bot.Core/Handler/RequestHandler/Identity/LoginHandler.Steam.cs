using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Handler.RequestHandler.Identity;

public class LoginSteamHandler : IRequestHandler<LoginSteamRequest, LoginSteamResponse>
{
    private readonly IIdentityService _service;

    public LoginSteamHandler(IIdentityService service)
    {
        _service = service;
    }
    
    public async Task<LoginSteamResponse> Handle(IReceiveContext<LoginSteamRequest> context, CancellationToken cancellationToken)
    {
        return await _service.LinkToSteamAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}