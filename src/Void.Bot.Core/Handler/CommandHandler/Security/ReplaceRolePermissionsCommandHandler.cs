using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class ReplaceRolePermissionsCommandHandler : ICommandHandler<ReplaceRolePermissionsCommand, ReplaceRolePermissionsResponse>
{
    private readonly IRolePermissionService _service;

    public ReplaceRolePermissionsCommandHandler(IRolePermissionService service)
    {
        _service = service;
    }

    public async Task<ReplaceRolePermissionsResponse> Handle(IReceiveContext<ReplaceRolePermissionsCommand> context, CancellationToken cancellationToken)
    {
        return await _service.ReplaceRolePermissionsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}