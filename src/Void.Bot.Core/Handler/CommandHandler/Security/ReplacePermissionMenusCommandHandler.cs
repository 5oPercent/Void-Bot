using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class ReplacePermissionMenusCommandHandler : ICommandHandler<ReplacePermissionMenusCommand, ReplacePermissionMenusResponse>
{
    private readonly IPermissionMenuService _service;
    
    public ReplacePermissionMenusCommandHandler(IPermissionMenuService service)
    {
        _service = service;
    }
    
    public async Task<ReplacePermissionMenusResponse> Handle(IReceiveContext<ReplacePermissionMenusCommand> context, CancellationToken cancellationToken)
    {
        return await _service.ReplacePermissionMenusAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}