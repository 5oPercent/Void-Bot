using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Handler.CommandHandler.Social;

public class SendFriendRequestCommandHandler : ICommandHandler<SendFriendRequestCommand, SendFriendRequestResponse>
{
    private readonly IFriendService _service;

    public SendFriendRequestCommandHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<SendFriendRequestResponse> Handle(IReceiveContext<SendFriendRequestCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.SendFriendRequestAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
