using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Handler.RequestHandler.Identity;

public class LoginBungieHandler : IRequestHandler<LoginBungieRequest, LoginBungieResponse>
{
    private readonly IIdentityService _service;

    public LoginBungieHandler(IIdentityService service)
    {
        _service = service;
    }
    
    public async Task<LoginBungieResponse> Handle(IReceiveContext<LoginBungieRequest> context, CancellationToken cancellationToken)
    {
        return await _service.LoginFromBungieAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}