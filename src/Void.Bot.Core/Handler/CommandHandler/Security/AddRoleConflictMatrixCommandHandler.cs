using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class AddRoleConflictMatrixCommandHandler : ICommandHandler<AddRoleConflictMatrixCommand, AddRoleConflictMatrixResponse>
{
    private readonly IRoleService _service;

    public AddRoleConflictMatrixCommandHandler(IRoleService service)
    {
        _service = service;
    }

    public async Task<AddRoleConflictMatrixResponse> Handle(IReceiveContext<AddRoleConflictMatrixCommand> context, CancellationToken cancellationToken)
    {
        return await _service.AddRoleConflictMatrixAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}