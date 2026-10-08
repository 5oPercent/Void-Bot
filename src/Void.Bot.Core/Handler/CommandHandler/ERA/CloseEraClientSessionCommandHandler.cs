using System.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class CloseEraClientSessionCommandHandler : ICommandHandler<CloseEraClientSessionCommand, CloseEraClientSessionResponse>
{
    private readonly IEraClientSessionService _service;

    public CloseEraClientSessionCommandHandler(IEraClientSessionService service)
    {
        _service = service;
    }

    public async Task<CloseEraClientSessionResponse> Handle(IReceiveContext<CloseEraClientSessionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.CloseAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
