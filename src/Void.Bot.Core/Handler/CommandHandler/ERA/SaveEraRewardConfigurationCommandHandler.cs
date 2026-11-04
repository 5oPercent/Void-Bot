using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Reward;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SaveEraRewardConfigurationCommandHandler : ICommandHandler<SaveEraRewardConfigurationCommand, SaveEraRewardConfigurationResponse>
{
    private readonly IEraInventoryRewardService _service;
    public SaveEraRewardConfigurationCommandHandler(IEraInventoryRewardService service)
    {
        _service = service;
    }

    public Task<SaveEraRewardConfigurationResponse> Handle(IReceiveContext<SaveEraRewardConfigurationCommand> context, CancellationToken cancellationToken)
    {
        return _service.SaveConfigurationAsync(context.Message, cancellationToken);
    }
}
