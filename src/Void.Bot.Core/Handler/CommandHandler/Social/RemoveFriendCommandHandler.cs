using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Handler.CommandHandler.Social;

public class RemoveFriendCommandHandler : ICommandHandler<RemoveFriendCommand, RemoveFriendResponse>
{
    private readonly IFriendService _service;

    public RemoveFriendCommandHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<RemoveFriendResponse> Handle(IReceiveContext<RemoveFriendCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.RemoveFriendAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
