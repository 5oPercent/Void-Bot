using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class RemovePermissionFromRoleCommandHandler : ICommandHandler<RemovePermissionFromRoleCommand, RemovePermissionFromRoleResponse>
{
    private readonly IRolePermissionService _service;

    public RemovePermissionFromRoleCommandHandler(IRolePermissionService service)
    {
        _service = service;
    }
    
    public async Task<RemovePermissionFromRoleResponse> Handle(IReceiveContext<RemovePermissionFromRoleCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RemovePermissionFromRoleAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}