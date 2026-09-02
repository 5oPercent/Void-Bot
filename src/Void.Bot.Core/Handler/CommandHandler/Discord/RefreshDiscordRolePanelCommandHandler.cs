using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord.RolePanel;
using Void.Bot.Message.Commands.Discord;

namespace Void.Bot.Core.Handler.CommandHandler.Discord;

public sealed class RefreshDiscordRolePanelCommandHandler(IDiscordRolePanelService service)
    : ICommandHandler<RefreshDiscordRolePanelCommand, RefreshDiscordRolePanelResponse>
{
    public Task<RefreshDiscordRolePanelResponse> Handle(
        IReceiveContext<RefreshDiscordRolePanelCommand> context,
        CancellationToken cancellationToken)
    {
        return service.RefreshAsync(context.Message, cancellationToken);
    }
}
