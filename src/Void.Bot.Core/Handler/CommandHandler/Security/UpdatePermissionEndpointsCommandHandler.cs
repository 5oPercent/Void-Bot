using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UpdatePermissionEndpointsCommandHandler : ICommandHandler<UpdatePermissionEndpointsCommand, UpdatePermissionEndpointsResponse>
{
    private readonly IPermissionEndpointService _service;

    public UpdatePermissionEndpointsCommandHandler(IPermissionEndpointService service)
    {
        _service = service;
    }
    
    public async Task<UpdatePermissionEndpointsResponse> Handle(IReceiveContext<UpdatePermissionEndpointsCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UpdatePermissionEndpointsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}