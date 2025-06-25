using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UpdateRolePermissionsCommandHandler : ICommandHandler<UpdateRolePermissionsCommand, UpdateRolePermissionsResponse>
{
    private readonly IRolePermissionService _service;

    public UpdateRolePermissionsCommandHandler(IRolePermissionService service)
    {
        _service = service;
    }

    public async Task<UpdateRolePermissionsResponse> Handle(IReceiveContext<UpdateRolePermissionsCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UpdateRolePermissionsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}