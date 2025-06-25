using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetEndpointListRequestHandler : IRequestHandler<GetEndpointListRequest, GetEndpointListResponse>
{
    private readonly IEndpointService _service;

    public GetEndpointListRequestHandler(IEndpointService service)
    {
        _service = service;
    }
    
    public async Task<GetEndpointListResponse> Handle(IReceiveContext<GetEndpointListRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetEndpointListAsync(context.Message).ConfigureAwait(false);
    }
}