using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Core.Handler.RequestHandler.Social;

public class GetAccountBlocksRequestHandler : IRequestHandler<GetAccountBlocksRequest, GetAccountBlocksResponse>
{
    private readonly IFriendService _service;

    public GetAccountBlocksRequestHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<GetAccountBlocksResponse> Handle(IReceiveContext<GetAccountBlocksRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.GetAccountBlocksAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
