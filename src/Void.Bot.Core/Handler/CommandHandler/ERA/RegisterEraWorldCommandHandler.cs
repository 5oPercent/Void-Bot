using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class RegisterEraWorldCommandHandler : ICommandHandler<RegisterEraWorldCommand, RegisterEraWorldResponse>
{
    private readonly IEraWorldService _service;

    public RegisterEraWorldCommandHandler(IEraWorldService service)
    {
        _service = service;
    }

    public Task<RegisterEraWorldResponse> Handle(IReceiveContext<RegisterEraWorldCommand> context, CancellationToken cancellationToken)
    {
        return _service.RegisterAsync(context.Message, cancellationToken);
    }
}
