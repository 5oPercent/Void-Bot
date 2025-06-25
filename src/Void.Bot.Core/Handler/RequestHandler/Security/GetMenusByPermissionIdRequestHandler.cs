using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetMenusByPermissionIdRequestHandler : IRequestHandler<GetMenusByPermissionIdRequest, GetMenusByPermissionIdResponse>
{
    private readonly IPermissionMenuService _service;

    public GetMenusByPermissionIdRequestHandler(IPermissionMenuService service)
    {
        _service = service;
    }
    
    public async Task<GetMenusByPermissionIdResponse> Handle(IReceiveContext<GetMenusByPermissionIdRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetMenusByPermissionIdAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}