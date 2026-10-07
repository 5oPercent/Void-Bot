using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraAdminPlayerRequestHandler : IRequestHandler<GetEraAdminPlayerRequest, GetEraAdminPlayerResponse>
{
    private readonly IEraService _service;

    public GetEraAdminPlayerRequestHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<GetEraAdminPlayerResponse> Handle(
        IReceiveContext<GetEraAdminPlayerRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetAdminPlayerAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
