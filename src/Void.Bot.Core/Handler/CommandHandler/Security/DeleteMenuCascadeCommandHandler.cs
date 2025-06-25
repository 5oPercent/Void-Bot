using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class DeleteMenuCascadeCommandHandler : ICommandHandler<DeleteMenuCascadeCommand, DeleteMenuCascadeResponse>
{
    private readonly IMenuService _service;

    public DeleteMenuCascadeCommandHandler(IMenuService service)
    {
        _service = service;
    }

    public async Task<DeleteMenuCascadeResponse> Handle(IReceiveContext<DeleteMenuCascadeCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteMenuCascadeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}