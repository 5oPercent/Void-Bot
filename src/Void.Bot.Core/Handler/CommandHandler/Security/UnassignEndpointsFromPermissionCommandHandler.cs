using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UnassignEndpointsFromPermissionCommandHandler : ICommandHandler<UnassignEndpointsFromPermissionCommand, UnassignEndpointsFromPermissionResponse>
{
    private readonly IPermissionEndpointService _service;

    public UnassignEndpointsFromPermissionCommandHandler(IPermissionEndpointService service)
    {
        _service = service;
    }
    
    public async Task<UnassignEndpointsFromPermissionResponse> Handle(IReceiveContext<UnassignEndpointsFromPermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UnassignEndpointsFromPermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}