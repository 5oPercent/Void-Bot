using Discord.WebSocket;

namespace Void.Bot.Core.Services.Discord.Context;

public interface IDiscordGuildContext : IScope
{
    ulong GuildId { get; set; }

    SocketGuild Guild { get; set; }

    SocketGuildUser BotMember { get; set; }
}

public sealed class DiscordGuildContext : IDiscordGuildContext
{
    public ulong GuildId { get; set; }

    public SocketGuild Guild { get; set; } = null!;

    public SocketGuildUser BotMember { get; set; } = null!;
}
