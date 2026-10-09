using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Core.Handler.RequestHandler.Social;

public class GetFriendsRequestHandler : IRequestHandler<GetFriendsRequest, GetFriendsResponse>
{
    private readonly IFriendService _service;

    public GetFriendsRequestHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<GetFriendsResponse> Handle(IReceiveContext<GetFriendsRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetFriendsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
