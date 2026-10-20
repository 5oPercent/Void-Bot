using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class SetEraWorldEquipmentRestrictionCommandHandler : ICommandHandler<SetEraWorldEquipmentRestrictionCommand, SetEraWorldEquipmentRestrictionResponse>
{
    private readonly IEraWorldService _service;

    public SetEraWorldEquipmentRestrictionCommandHandler(IEraWorldService service) { _service = service; }

    public Task<SetEraWorldEquipmentRestrictionResponse> Handle(IReceiveContext<SetEraWorldEquipmentRestrictionCommand> context, CancellationToken cancellationToken) =>
        _service.ExecuteAsync<SetEraWorldEquipmentRestrictionResponse>(context.Message, cancellationToken);
}
