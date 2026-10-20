using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class PrepareEraInventoryRewardCommandHandler : ICommandHandler<PrepareEraInventoryRewardCommand, PrepareEraInventoryRewardResponse>
{
    private readonly IEraInventoryRewardService _service;
    public PrepareEraInventoryRewardCommandHandler(IEraInventoryRewardService service)
    {
        _service = service;
    }

    public Task<PrepareEraInventoryRewardResponse> Handle(IReceiveContext<PrepareEraInventoryRewardCommand> context, CancellationToken cancellationToken)
    {
        return _service.PrepareAsync(context.Message, cancellationToken);
    }
}
