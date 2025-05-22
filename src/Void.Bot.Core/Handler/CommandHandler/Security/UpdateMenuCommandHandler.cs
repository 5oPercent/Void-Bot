using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;

namespace Void.Bot.Core.Handler.CommandHandler.Security;

public class UpdateMenuCommandHandler : ICommandHandler<UpdateMenuCommand, UpdateMenuResponse>
{
    private readonly IMenuService _service;

    public UpdateMenuCommandHandler(IMenuService service)
    {
        _service = service;
    }

    public async Task<UpdateMenuResponse> Handle(IReceiveContext<UpdateMenuCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UpdateMenuAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}