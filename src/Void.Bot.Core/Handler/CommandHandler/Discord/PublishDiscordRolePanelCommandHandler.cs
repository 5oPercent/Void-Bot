using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord.RolePanel;
using Void.Bot.Message.Commands.Discord;

namespace Void.Bot.Core.Handler.CommandHandler.Discord;

public sealed class PublishDiscordRolePanelCommandHandler(IDiscordRolePanelService service)
    : ICommandHandler<PublishDiscordRolePanelCommand, PublishDiscordRolePanelResponse>
{
    public Task<PublishDiscordRolePanelResponse> Handle(
        IReceiveContext<PublishDiscordRolePanelCommand> context,
        CancellationToken cancellationToken)
    {
        return service.PublishAsync(context.Message, cancellationToken);
    }
}
