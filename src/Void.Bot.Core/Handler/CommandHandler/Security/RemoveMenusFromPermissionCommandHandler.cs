using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class RemoveMenusFromPermissionCommandHandler : ICommandHandler<RemoveMenusFromPermissionCommand, RemoveMenusFromPermissionResponse>
{
    private readonly IPermissionMenuService _service;
    
    public RemoveMenusFromPermissionCommandHandler(IPermissionMenuService service)
    {
        _service = service;
    }
    
    public async Task<RemoveMenusFromPermissionResponse> Handle(IReceiveContext<RemoveMenusFromPermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RemoveMenusFromPermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}