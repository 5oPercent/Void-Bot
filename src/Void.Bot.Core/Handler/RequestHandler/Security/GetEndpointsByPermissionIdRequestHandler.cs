using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetEndpointsByPermissionIdRequestHandler : IRequestHandler<GetEndpointsByPermissionIdRequest, GetEndpointsByPermissionIdResponse>
{
    private readonly IPermissionEndpointService _service;

    public GetEndpointsByPermissionIdRequestHandler(IPermissionEndpointService service)
    {
        _service = service;
    }
    
    public async Task<GetEndpointsByPermissionIdResponse> Handle(IReceiveContext<GetEndpointsByPermissionIdRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetEndpointsByPermissionIdAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}