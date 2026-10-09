using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Handler.CommandHandler.Social;

public class AcceptFriendRequestCommandHandler : ICommandHandler<AcceptFriendRequestCommand, AcceptFriendRequestResponse>
{
    private readonly IFriendService _service;

    public AcceptFriendRequestCommandHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<AcceptFriendRequestResponse> Handle(IReceiveContext<AcceptFriendRequestCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.AcceptFriendRequestAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
