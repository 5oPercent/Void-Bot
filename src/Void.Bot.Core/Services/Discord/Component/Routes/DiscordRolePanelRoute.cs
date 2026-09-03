using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Void.Bot.Message.Constants.Discord;
using Void.Bot.Core.Services.Discord.RolePanel;

namespace Void.Bot.Core.Services.Discord.Component.Routes;

public sealed class DiscordRolePanelRoute(
    IServiceScopeFactory scopeFactory) : DiscordComponentRouteBase
{
    protected override string CustomIdPrefix =>
        DiscordConstants.RolePanelButtonCustomIdPrefix;

    public override async Task ExecuteAsync(SocketMessageComponent callback)
    {
        using var scope = scopeFactory.CreateScope();
        await scope.ServiceProvider
            .GetRequiredService<IDiscordRolePanelService>()
            .HandleButtonAsync(callback)
            .ConfigureAwait(false);
    }
}