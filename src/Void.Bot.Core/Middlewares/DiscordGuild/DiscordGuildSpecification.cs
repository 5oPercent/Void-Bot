using System.Runtime.ExceptionServices;
using Discord.WebSocket;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Core.Services.Discord.Context;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Middlewares.DiscordGuild;

public sealed class DiscordGuildSpecification<TContext>(
    DiscordSocketClient client,
    IDiscordGuildContext guildContext,
    IDiscordService discordService)
    : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return context.Message is IRequireDiscordGuild;
    }

    public Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task Execute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken))
        {
            return;
        }

        var request = (IRequireDiscordGuild)context.Message;
        if (!ulong.TryParse(request.GuildId, out var guildId) || client.GetGuild(guildId) is not { } guild)
        {
            throw new DiscordGuildUnavailableException();
        }

        guildContext.Guild = guild;
        guildContext.GuildId = guild.Id;
        guildContext.BotMember = guild.CurrentUser;

        var managedGuilds = await discordService
            .GetManagedGuildListAsync(
                new GetDiscordManagedGuildListRequest(),
                cancellationToken)
            .ConfigureAwait(false);
        if (managedGuilds.Data?.All(x => x.Id != request.GuildId) != false)
        {
            throw new DiscordGuildManagementRequiredException();
        }

    }

    public Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnException(Exception ex, TContext context)
    {
        ExceptionDispatchInfo.Capture(ex).Throw();
        return Task.CompletedTask;
    }
}
