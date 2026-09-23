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
        this IPipeConfigurator<TContext> configurator,
        Discord.WebSocket.DiscordSocketClient? client = null,
        IDiscordGuildContext? guildContext = null,
        Func<IDiscordService>? discordService = null)
        where TContext : IContext<IMessage>
    {
        if ((client == null || guildContext == null || discordService == null) &&
            configurator.DependencyScope == null)
        {
            throw new DependencyScopeNotConfiguredException(
                "Discord guild middleware dependencies are not configured.");
        }

        client ??= configurator.DependencyScope.Resolve<Discord.WebSocket.DiscordSocketClient>();
        guildContext ??= configurator.DependencyScope.Resolve<IDiscordGuildContext>();
        discordService ??= configurator.DependencyScope.Resolve<Func<IDiscordService>>();

        configurator.AddPipeSpecification(new DiscordGuildSpecification<TContext>(client, guildContext, discordService));
    }
}
