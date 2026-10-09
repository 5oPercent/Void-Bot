using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Handler.CommandHandler.Social;

public class RejectFriendRequestCommandHandler : ICommandHandler<RejectFriendRequestCommand, RejectFriendRequestResponse>
{
    private readonly IFriendService _service;

    public RejectFriendRequestCommandHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<RejectFriendRequestResponse> Handle(IReceiveContext<RejectFriendRequestCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.RejectFriendRequestAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
