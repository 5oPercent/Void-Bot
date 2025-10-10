using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class AssignEndpointsToPermissionCommandHandler : ICommandHandler<AssignEndpointsToPermissionCommand, AssignEndpointsToPermissionResponse>
{
    private readonly IPermissionEndpointService _service;

    public AssignEndpointsToPermissionCommandHandler(IPermissionEndpointService service)
    {
        _service = service;
    }
    
    public async Task<AssignEndpointsToPermissionResponse> Handle(IReceiveContext<AssignEndpointsToPermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.AssignEndpointsToPermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}