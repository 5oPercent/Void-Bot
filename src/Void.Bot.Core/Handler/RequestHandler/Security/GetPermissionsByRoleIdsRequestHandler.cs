using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetPermissionsByRoleIdsRequestHandler : IRequestHandler<GetPermissionsByRoleIdsRequest, GetPermissionsByRoleIdsResponse>
{
    private readonly IRolePermissionService _service;

    public GetPermissionsByRoleIdsRequestHandler(IRolePermissionService service)
    {
        _service = service;
    }
    
    public async Task<GetPermissionsByRoleIdsResponse> Handle(IReceiveContext<GetPermissionsByRoleIdsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetPermissionsByRoleIdsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}