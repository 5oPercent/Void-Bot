using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Reward;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class GrantEraInventoryRewardCommandHandler : ICommandHandler<GrantEraInventoryRewardCommand, GrantEraInventoryRewardResponse>
{
    private readonly IEraInventoryRewardService _service;
    public GrantEraInventoryRewardCommandHandler(IEraInventoryRewardService service)
    {
        _service = service;
    }

    public Task<GrantEraInventoryRewardResponse> Handle(IReceiveContext<GrantEraInventoryRewardCommand> context, CancellationToken cancellationToken)
    {
        return _service.GrantAsync(context.Message, cancellationToken);
    }
}
