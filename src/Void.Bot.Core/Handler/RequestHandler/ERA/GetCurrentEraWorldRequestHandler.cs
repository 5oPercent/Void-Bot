using System.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetCurrentEraWorldRequestHandler : IRequestHandler<GetCurrentEraWorldRequest, GetCurrentEraWorldResponse>
{
    private readonly IEraWorldService _service;

    public GetCurrentEraWorldRequestHandler(IEraWorldService service)
    {
        _service = service;
    }

    public async Task<GetCurrentEraWorldResponse> Handle(IReceiveContext<GetCurrentEraWorldRequest> context, CancellationToken cancellationToken)
    {
        return new GetCurrentEraWorldResponse { Code = HttpStatusCode.OK, Data = await _service.GetAsync(null, cancellationToken).ConfigureAwait(false) };
    }
}
