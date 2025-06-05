using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class AssignPermissionToRoleCommandHandler : ICommandHandler<AssignPermissionToRoleCommand, AssignPermissionToRoleResponse>
{
    private readonly IRolePermissionService _service;

    public AssignPermissionToRoleCommandHandler(IRolePermissionService service)
    {
        _service = service;
    }
    
    public async Task<AssignPermissionToRoleResponse> Handle(IReceiveContext<AssignPermissionToRoleCommand> context, CancellationToken cancellationToken)
    {
        return await _service.AssignPermissionToRoleAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}