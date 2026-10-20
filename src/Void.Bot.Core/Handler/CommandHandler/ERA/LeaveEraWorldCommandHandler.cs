using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class LeaveEraWorldCommandHandler : ICommandHandler<LeaveEraWorldCommand, LeaveEraWorldResponse>
{
    private readonly IEraWorldService _service;

    public LeaveEraWorldCommandHandler(IEraWorldService service) { _service = service; }

    public Task<LeaveEraWorldResponse> Handle(IReceiveContext<LeaveEraWorldCommand> context, CancellationToken cancellationToken) =>
        _service.ExecuteAsync<LeaveEraWorldResponse>(context.Message, cancellationToken);
}
