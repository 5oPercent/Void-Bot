using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Handler.CommandHandler.Social;

public class UnblockAccountCommandHandler : ICommandHandler<UnblockAccountCommand, UnblockAccountResponse>
{
    private readonly IFriendService _service;

    public UnblockAccountCommandHandler(IFriendService service)
    {
        _service = service;
    }

    public async Task<UnblockAccountResponse> Handle(IReceiveContext<UnblockAccountCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.UnblockAccountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
