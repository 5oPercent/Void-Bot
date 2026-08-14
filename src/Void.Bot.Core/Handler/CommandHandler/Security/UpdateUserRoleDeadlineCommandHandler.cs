using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UpdateUserRoleDeadlineCommandHandler : ICommandHandler<UpdateUserRoleDeadlineCommand, UpdateUserRoleDeadlineResponse>
{
    private readonly IUserRoleService _service;

    public UpdateUserRoleDeadlineCommandHandler(IUserRoleService service)
    {
        _service = service;
    }

    public async Task<UpdateUserRoleDeadlineResponse> Handle(
        IReceiveContext<UpdateUserRoleDeadlineCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UpdateUserRoleDeadlineAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
    }
}