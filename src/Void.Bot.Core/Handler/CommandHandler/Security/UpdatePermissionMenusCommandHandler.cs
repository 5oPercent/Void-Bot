using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UpdatePermissionMenusCommandHandler : ICommandHandler<UpdatePermissionMenusCommand, UpdatePermissionMenusResponse>
{
    private readonly IPermissionMenuService _service;
    
    public UpdatePermissionMenusCommandHandler(IPermissionMenuService service)
    {
        _service = service;
    }
    
    public async Task<UpdatePermissionMenusResponse> Handle(IReceiveContext<UpdatePermissionMenusCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UpdatePermissionMenusAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}