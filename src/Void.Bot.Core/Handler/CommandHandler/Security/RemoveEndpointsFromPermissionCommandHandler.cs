using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class RemoveEndpointsFromPermissionCommandHandler : ICommandHandler<RemoveEndpointsFromPermissionCommand, RemoveEndpointsFromPermissionResponse>
{
    private readonly IPermissionEndpointService _service;

    public RemoveEndpointsFromPermissionCommandHandler(IPermissionEndpointService service)
    {
        _service = service;
    }
    
    public async Task<RemoveEndpointsFromPermissionResponse> Handle(IReceiveContext<RemoveEndpointsFromPermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RemoveEndpointsFromPermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}