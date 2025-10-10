using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class RemoveRolesCascadeUserCommandHandler : ICommandHandler<RemoveRolesCascadeUserCommand, RemoveRolesCascadeUserResponse>
{
    private readonly IUserRoleService _service;

    public RemoveRolesCascadeUserCommandHandler(IUserRoleService service)
    {
        _service = service;
    }
    
    public async Task<RemoveRolesCascadeUserResponse> Handle(IReceiveContext<RemoveRolesCascadeUserCommand> context, CancellationToken cancellationToken)
    {
        return await _service.RemoveRolesCascadeUserAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}