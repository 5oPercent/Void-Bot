using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord.RolePanel;
using Void.Bot.Message.Commands.Discord;

namespace Void.Bot.Core.Handler.CommandHandler.Discord;

public sealed class UnpublishDiscordRolePanelCommandHandler(IDiscordRolePanelService service)
    : ICommandHandler<UnpublishDiscordRolePanelCommand, UnpublishDiscordRolePanelResponse>
{
    public Task<UnpublishDiscordRolePanelResponse> Handle(
        IReceiveContext<UnpublishDiscordRolePanelCommand> context,
        CancellationToken cancellationToken)
    {
        return service.UnpublishAsync(context.Message, cancellationToken);
    }
}
