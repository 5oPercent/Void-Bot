using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UnassignPermissionsFromRoleCommandHandler : ICommandHandler<UnassignPermissionsFromRoleCommand, UnassignPermissionsFromRoleResponse>
{
    private readonly IRolePermissionService _service;

    public UnassignPermissionsFromRoleCommandHandler(IRolePermissionService service)
    {
        _service = service;
    }
    
    public async Task<UnassignPermissionsFromRoleResponse> Handle(IReceiveContext<UnassignPermissionsFromRoleCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UnassignPermissionsFromRoleAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}