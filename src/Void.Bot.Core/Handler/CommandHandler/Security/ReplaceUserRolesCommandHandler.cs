using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class ReplaceUserRolesCommandHandler : ICommandHandler<ReplaceUserRolesCommand, ReplaceUserRolesResponse>
{
    private readonly IUserRoleService _service;

    public ReplaceUserRolesCommandHandler(IUserRoleService service)
    {
        _service = service;
    }
    
    public async Task<ReplaceUserRolesResponse> Handle(IReceiveContext<ReplaceUserRolesCommand> context, CancellationToken cancellationToken)
    {
        return await _service.ReplaceUserRolesAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}