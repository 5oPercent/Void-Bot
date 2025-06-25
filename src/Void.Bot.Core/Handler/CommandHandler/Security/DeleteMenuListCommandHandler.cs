using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class DeleteMenuListCommandHandler : ICommandHandler<DeleteMenuListCommand, DeleteMenuListResponse>
{
    private readonly IMenuService _service;

    public DeleteMenuListCommandHandler(IMenuService service)
    {
        _service = service;
    }

    public async Task<DeleteMenuListResponse> Handle(IReceiveContext<DeleteMenuListCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteMenuListAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}