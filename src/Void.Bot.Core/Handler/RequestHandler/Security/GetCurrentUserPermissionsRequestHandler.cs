using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetCurrentUserPermissionsRequestHandler : IRequestHandler<GetCurrentUserPermissionsRequest, GetCurrentUserPermissionsResponse>
{
    private readonly ISecurityService _service;

    public GetCurrentUserPermissionsRequestHandler(ISecurityService service)
    {
        _service = service;
    }

    public async Task<GetCurrentUserPermissionsResponse> Handle(
        IReceiveContext<GetCurrentUserPermissionsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetCurrentUserPermissionsAsync(cancellationToken).ConfigureAwait(false);
    }
}