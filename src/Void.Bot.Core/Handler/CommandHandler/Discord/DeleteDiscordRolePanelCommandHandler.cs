using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord.RolePanel;
using Void.Bot.Message.Commands.Discord;

namespace Void.Bot.Core.Handler.CommandHandler.Discord;

public sealed class DeleteDiscordRolePanelCommandHandler(IDiscordRolePanelService service)
    : ICommandHandler<DeleteDiscordRolePanelCommand, DeleteDiscordRolePanelResponse>
{
    public Task<DeleteDiscordRolePanelResponse> Handle(
        IReceiveContext<DeleteDiscordRolePanelCommand> context,
        CancellationToken cancellationToken)
    {
        return service.DeleteConfigAsync(context.Message, cancellationToken);
    }
}
