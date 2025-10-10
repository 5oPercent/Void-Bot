using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetRolesByUserIdRequestHandler : IRequestHandler<GetRolesByUserIdRequest, GetRolesByUserIdResponse>
{
    private readonly IUserRoleService _service;

    public GetRolesByUserIdRequestHandler(IUserRoleService service)
    {
        _service = service;
    }
    
    public async Task<GetRolesByUserIdResponse> Handle(IReceiveContext<GetRolesByUserIdRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetRolesByUserIdAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}