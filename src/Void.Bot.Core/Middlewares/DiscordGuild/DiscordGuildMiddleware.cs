using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Core.Services.Discord.Context;

namespace Void.Bot.Core.Middlewares.DiscordGuild;

public static class DiscordGuildMiddleware
{
    public static void UseDiscordGuild<TContext>(
        this IPipeConfigurator<TContext> configurator)
        where TContext : IContext<IMessage>
    {
        if (configurator.DependencyScope == null)
        {
            throw new DependencyScopeNotConfiguredException(
                "Discord guild middleware dependencies are not configured.");
        }

        var client = configurator.DependencyScope.Resolve<Discord.WebSocket.DiscordSocketClient>();
        var guildContext = configurator.DependencyScope.Resolve<IDiscordGuildContext>();
        var discordService = configurator.DependencyScope.Resolve<IDiscordService>();
        configurator.AddPipeSpecification(new DiscordGuildSpecification<TContext>(client, guildContext, discordService));
    }
}
