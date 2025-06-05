using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetRoleHasPermissionRequestHandler : IRequestHandler<GetRoleHasPermissionRequest, GetRoleHasPermissionResponse>
{
    private readonly IRolePermissionService _service;

    public GetRoleHasPermissionRequestHandler(IRolePermissionService service)
    {
        _service = service;
    }
    
    public async Task<GetRoleHasPermissionResponse> Handle(IReceiveContext<GetRoleHasPermissionRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetRoleHasPermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}