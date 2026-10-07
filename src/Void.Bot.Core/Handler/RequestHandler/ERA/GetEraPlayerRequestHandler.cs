using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraPlayerRequestHandler : IRequestHandler<GetEraPlayerRequest, GetEraPlayerResponse>
{
    private readonly IEraService _service;

    public GetEraPlayerRequestHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<GetEraPlayerResponse> Handle(
        IReceiveContext<GetEraPlayerRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetPlayerAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
