using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetPermissionsByRoleIdRequestHandler : IRequestHandler<GetPermissionsByRoleIdRequest, GetPermissionsByRoleIdResponse>
{
    private readonly IRolePermissionService _service;

    public GetPermissionsByRoleIdRequestHandler(IRolePermissionService service)
    {
        _service = service;
    }
    
    public async Task<GetPermissionsByRoleIdResponse> Handle(IReceiveContext<GetPermissionsByRoleIdRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetPermissionsByRoleIdAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}