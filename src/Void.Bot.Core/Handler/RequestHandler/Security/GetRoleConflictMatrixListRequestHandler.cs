using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetRoleConflictMatrixListRequestHandler : IRequestHandler<GetRoleConflictMatrixListRequest, GetRoleConflictMatrixListResponse>
{
    private readonly IRoleService _service;

    public GetRoleConflictMatrixListRequestHandler(IRoleService service)
    {
        _service = service;
    }
    
    public async Task<GetRoleConflictMatrixListResponse> Handle(IReceiveContext<GetRoleConflictMatrixListRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetRoleConflictMatrixListAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}