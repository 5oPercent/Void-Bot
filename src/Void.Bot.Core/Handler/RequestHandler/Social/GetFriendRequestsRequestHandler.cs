using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Core.Handler.RequestHandler.Social;

public class GetFriendRequestsRequestHandler : IRequestHandler<GetFriendRequestsRequest, GetFriendRequestsResponse>
{
    private readonly IFriendService _service;

    public GetFriendRequestsRequestHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<GetFriendRequestsResponse> Handle(IReceiveContext<GetFriendRequestsRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetFriendRequestsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
