using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord.RolePanel;
using Void.Bot.Message.Commands.Discord;

namespace Void.Bot.Core.Handler.CommandHandler.Discord;

public sealed class SaveDiscordRolePanelCommandHandler(IDiscordRolePanelService service)
    : ICommandHandler<SaveDiscordRolePanelCommand, SaveDiscordRolePanelResponse>
{
    public Task<SaveDiscordRolePanelResponse> Handle(
        IReceiveContext<SaveDiscordRolePanelCommand> context,
        CancellationToken cancellationToken)
    {
        return service.SaveConfigAsync(context.Message, cancellationToken);
    }
}
