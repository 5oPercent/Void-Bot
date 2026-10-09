using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraFriendStatusesRequestHandler : IRequestHandler<GetEraFriendStatusesRequest, GetEraFriendStatusesResponse>
{
    private readonly IEraSocialService _service;

    public GetEraFriendStatusesRequestHandler(IEraSocialService service)
    {
        _service = service;
    }

    public async Task<GetEraFriendStatusesResponse> Handle(IReceiveContext<GetEraFriendStatusesRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetEraFriendStatusesAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
