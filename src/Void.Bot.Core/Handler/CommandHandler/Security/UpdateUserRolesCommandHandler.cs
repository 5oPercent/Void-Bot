using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UpdateUserRolesCommandHandler : ICommandHandler<UpdateUserRolesCommand, UpdateUserRolesResponse>
{
    private readonly IUserRoleService _service;

    public UpdateUserRolesCommandHandler(IUserRoleService service)
    {
        _service = service;
    }
    
    public async Task<UpdateUserRolesResponse> Handle(IReceiveContext<UpdateUserRolesCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UpdateUserRolesAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}