using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Handler.CommandHandler.Social;

public class BlockAccountCommandHandler : ICommandHandler<BlockAccountCommand, BlockAccountResponse>
{
    private readonly IFriendService _service;

    public BlockAccountCommandHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<BlockAccountResponse> Handle(IReceiveContext<BlockAccountCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.BlockAccountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
