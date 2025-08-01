using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class AssignRolesToUserCommandHandler : ICommandHandler<AssignRolesToUserCommand, AssignRolesToUserResponse>
{
    private readonly IUserRoleService _service;

    public AssignRolesToUserCommandHandler(IUserRoleService service)
    {
        _service = service;
    }
    
    public async Task<AssignRolesToUserResponse> Handle(IReceiveContext<AssignRolesToUserCommand> context, CancellationToken cancellationToken)
    {
        return await _service.AssignRolesToUserAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}