using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UnassignMenusFromPermissionCommandHandler : ICommandHandler<UnassignMenusFromPermissionCommand, UnassignMenusFromPermissionResponse>
{
    private readonly IPermissionMenuService _service;

    public UnassignMenusFromPermissionCommandHandler(IPermissionMenuService service)
    {
        _service = service;
    }
    
    public async Task<UnassignMenusFromPermissionResponse> Handle(IReceiveContext<UnassignMenusFromPermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UnassignMenusFromPermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}