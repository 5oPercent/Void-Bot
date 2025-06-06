using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class RemoveRoleCascadePermissionCommandHandler : ICommandHandler<RemoveRoleCascadePermissionCommand, RemoveRoleCascadePermissionResponse>
{
    private readonly IRolePermissionService _service;

    public RemoveRoleCascadePermissionCommandHandler(IRolePermissionService service)
    {
        _service = service;
    }
    
    public async Task<RemoveRoleCascadePermissionResponse> Handle(IReceiveContext<RemoveRoleCascadePermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RemoveRoleCascadePermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}