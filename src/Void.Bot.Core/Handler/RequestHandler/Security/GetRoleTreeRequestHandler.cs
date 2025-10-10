using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetRoleTreeRequestHandler : IRequestHandler<GetRoleTreeRequest, GetRoleTreeResponse>
{
    private readonly IRoleService _service;

    public GetRoleTreeRequestHandler(IRoleService service)
    {
        _service = service;
    }
    
    public async Task<GetRoleTreeResponse> Handle(IReceiveContext<GetRoleTreeRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetRoleTreeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}