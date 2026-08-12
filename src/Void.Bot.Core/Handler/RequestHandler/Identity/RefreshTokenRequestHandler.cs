using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Handler.RequestHandler.Identity;

public class RefreshTokenRequestHandler : IRequestHandler<RefreshTokenRequest, RefreshTokenResponse>
{
    private readonly IIdentityService _service;

    public RefreshTokenRequestHandler(IIdentityService service)
    {
        _service = service;
    }

    public async Task<RefreshTokenResponse> Handle(
        IReceiveContext<RefreshTokenRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.RefreshTokenAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
