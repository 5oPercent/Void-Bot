using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class AssignMenusToPermissionCommandHandler : ICommandHandler<AssignMenusToPermissionCommand, AssignMenusToPermissionResponse>
{
    private readonly IPermissionMenuService _service;
    
    public AssignMenusToPermissionCommandHandler(IPermissionMenuService service)
    {
        _service = service;
    }
    
    public async Task<AssignMenusToPermissionResponse> Handle(IReceiveContext<AssignMenusToPermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.AssignMenusToPermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}