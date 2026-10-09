using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Handler.CommandHandler.Social;

public class WithdrawFriendRequestCommandHandler : ICommandHandler<WithdrawFriendRequestCommand, WithdrawFriendRequestResponse>
{
    private readonly IFriendService _service;

    public WithdrawFriendRequestCommandHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<WithdrawFriendRequestResponse> Handle(IReceiveContext<WithdrawFriendRequestCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.WithdrawFriendRequestAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
