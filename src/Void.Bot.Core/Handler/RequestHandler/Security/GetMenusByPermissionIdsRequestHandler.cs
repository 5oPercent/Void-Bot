using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetMenusByPermissionIdsRequestHandler : IRequestHandler<GetMenusByPermissionIdsRequest, GetMenusByPermissionIdsResponse>
{
    private readonly IPermissionMenuService _service;

    public GetMenusByPermissionIdsRequestHandler(IPermissionMenuService service)
    {
        _service = service;
    }
    
    public async Task<GetMenusByPermissionIdsResponse> Handle(IReceiveContext<GetMenusByPermissionIdsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetMenusByPermissionIdsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}