using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class RemoveEndpointsCascadePermissionCommandHandler : ICommandHandler<RemoveEndpointsCascadePermissionCommand, RemoveEndpointsCascadePermissionResponse>
{
    private readonly IPermissionEndpointService _service;

    public RemoveEndpointsCascadePermissionCommandHandler(IPermissionEndpointService service)
    {
        _service = service;
    }
    
    public async Task<RemoveEndpointsCascadePermissionResponse> Handle(IReceiveContext<RemoveEndpointsCascadePermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RemoveEndpointsCascadePermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}