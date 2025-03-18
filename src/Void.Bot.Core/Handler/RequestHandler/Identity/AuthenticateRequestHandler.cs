using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Handler.RequestHandler.Identity;

public class AuthenticateRequestHandler : IRequestHandler<AuthenticateRequest, AuthenticateResponse>
{
    private readonly IIdentityService _service;

    public AuthenticateRequestHandler(IIdentityService service)
    {
        _service = service;
    }
    
    public async Task<AuthenticateResponse> Handle(IReceiveContext<AuthenticateRequest> context, CancellationToken cancellationToken)
    {
        return await _service.AuthenticateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}