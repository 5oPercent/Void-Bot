using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Reward;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class PublishEraRewardConfigurationCommandHandler : ICommandHandler<PublishEraRewardConfigurationCommand, PublishEraRewardConfigurationResponse>
{
    private readonly IEraInventoryRewardService _service;
    public PublishEraRewardConfigurationCommandHandler(IEraInventoryRewardService service)
    {
        _service = service;
    }

    public Task<PublishEraRewardConfigurationResponse> Handle(IReceiveContext<PublishEraRewardConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return _service.PublishAsync(context.Message, cancellationToken);
    }
}
