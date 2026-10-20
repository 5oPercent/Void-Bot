using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraCharacterRequestHandler : IRequestHandler<GetEraCharacterRequest, GetEraCharacterResponse>
{
    private readonly IEraService _service;

    public GetEraCharacterRequestHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<GetEraCharacterResponse> Handle(
        IReceiveContext<GetEraCharacterRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetCharacterAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
