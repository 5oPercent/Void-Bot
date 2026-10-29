using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Drop;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class DestroyEraEquipmentDropsCommandHandler : ICommandHandler<DestroyEraEquipmentDropsCommand, DestroyEraEquipmentDropsResponse>
{
    private readonly IEraEquipmentDropService _service;

    public DestroyEraEquipmentDropsCommandHandler(IEraEquipmentDropService service)
    {
        _service = service;
    }

    public Task<DestroyEraEquipmentDropsResponse> Handle(IReceiveContext<DestroyEraEquipmentDropsCommand> context, CancellationToken cancellationToken)
    {
        return _service.DestroyAsync(context.Message, cancellationToken);
    }
}
