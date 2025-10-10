using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class AddPermissionCommandHandler : ICommandHandler<AddPermissionCommand, AddPermissionResponse>
{
    private readonly IPermissionService _service;

    public AddPermissionCommandHandler(IPermissionService service)
    {
        _service = service;
    }
    
    public async Task<AddPermissionResponse> Handle(IReceiveContext<AddPermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _service.AddPermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}