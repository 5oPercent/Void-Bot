using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Drop;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class GrantEraGroundRewardCommandHandler : ICommandHandler<GrantEraGroundRewardCommand, SubmitEraGroundRewardResponse>
{
    private readonly IEraEquipmentDropService _service;

    public GrantEraGroundRewardCommandHandler(IEraEquipmentDropService service)
    {
        _service = service;
    }

    public Task<SubmitEraGroundRewardResponse> Handle(IReceiveContext<GrantEraGroundRewardCommand> context, CancellationToken cancellationToken)
    {
        return _service.GrantAsync(context.Message, cancellationToken);
    }
}
