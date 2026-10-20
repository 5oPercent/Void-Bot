using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Drop;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SubmitEraGroundRewardCommandHandler : ICommandHandler<SubmitEraGroundRewardCommand, SubmitEraGroundRewardResponse>
{
    private readonly IEraEquipmentDropService _service;

    public SubmitEraGroundRewardCommandHandler(IEraEquipmentDropService service)
    {
        _service = service;
    }

    public Task<SubmitEraGroundRewardResponse> Handle(IReceiveContext<SubmitEraGroundRewardCommand> context, CancellationToken cancellationToken)
    {
        return _service.SubmitAsync(context.Message, cancellationToken);
    }
}
