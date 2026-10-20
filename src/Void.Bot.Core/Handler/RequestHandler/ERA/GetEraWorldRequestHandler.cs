using System.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWorldRequestHandler : IRequestHandler<GetEraWorldRequest, GetEraWorldResponse>
{
    private readonly IEraWorldService _service;

    public GetEraWorldRequestHandler(IEraWorldService service)
    {
        _service = service;
    }

    public async Task<GetEraWorldResponse> Handle(IReceiveContext<GetEraWorldRequest> context, CancellationToken cancellationToken)
    {
        return new GetEraWorldResponse { Code = HttpStatusCode.OK, Data = await _service.GetAsync(context.Message.WorldInstanceId, cancellationToken).ConfigureAwait(false) };
    }
}
