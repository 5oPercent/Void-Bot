using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Core.Handler.RequestHandler.Social;

public class FindSocialAccountRequestHandler : IRequestHandler<FindSocialAccountRequest, FindSocialAccountResponse>
{
    private readonly IFriendService _service;

    public FindSocialAccountRequestHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<FindSocialAccountResponse> Handle(IReceiveContext<FindSocialAccountRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.FindSocialAccountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
