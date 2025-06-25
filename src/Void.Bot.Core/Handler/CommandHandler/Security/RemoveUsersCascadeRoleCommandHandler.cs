using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class RemoveUsersCascadeRoleCommandHandler : ICommandHandler<RemoveUsersCascadeRoleCommand, RemoveUsersCascadeRoleResponse>
{
    private readonly IUserRoleService _service;

    public RemoveUsersCascadeRoleCommandHandler(IUserRoleService service)
    {
        _service = service;
    }
    
    public async Task<RemoveUsersCascadeRoleResponse> Handle(IReceiveContext<RemoveUsersCascadeRoleCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RemoveUsersCascadeRoleAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}