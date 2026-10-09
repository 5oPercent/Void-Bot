using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraFriendTargetRequestHandler : IRequestHandler<GetEraFriendTargetRequest, GetEraFriendTargetResponse>
{
    private readonly IEraSocialService _service;

    public GetEraFriendTargetRequestHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<GetEraFriendTargetResponse> Handle(IReceiveContext<GetEraFriendTargetRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetEraFriendTargetAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
