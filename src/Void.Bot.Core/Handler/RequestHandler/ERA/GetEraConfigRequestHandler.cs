using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraConfigRequestHandler : IRequestHandler<GetEraConfigRequest, GetEraConfigResponse>
{
    private readonly IEraService _service;

    public GetEraConfigRequestHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<GetEraConfigResponse> Handle(
        IReceiveContext<GetEraConfigRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetConfigAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
