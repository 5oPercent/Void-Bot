using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UnassignRolesFromUserCommandHandler : ICommandHandler<UnassignRolesFromUserCommand, UnassignRolesFromUserResponse>
{
    private readonly IUserRoleService _service;

    public UnassignRolesFromUserCommandHandler(IUserRoleService service)
    {
        _service = service;
    }
    
    public async Task<UnassignRolesFromUserResponse> Handle(IReceiveContext<UnassignRolesFromUserCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UnassignRolesFromUserAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}