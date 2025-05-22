using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class DeleteRoleListCommandHandler : ICommandHandler<DeleteRoleListCommand, DeleteRoleListResponse>
{
    private readonly IRoleService _service;

    public DeleteRoleListCommandHandler(IRoleService service)
    {
        _service = service;
    }

    public async Task<DeleteRoleListResponse> Handle(IReceiveContext<DeleteRoleListCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteRoleListAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}