using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class DeletePermissionListCommandHandler : ICommandHandler<DeletePermissionListCommand, DeletePermissionListResponse>
{
    private readonly IPermissionService _service;

    public DeletePermissionListCommandHandler(IPermissionService service)
    {
        _service = service;
    }
    
    public async Task<DeletePermissionListResponse> Handle(IReceiveContext<DeletePermissionListCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeletePermissionListAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}