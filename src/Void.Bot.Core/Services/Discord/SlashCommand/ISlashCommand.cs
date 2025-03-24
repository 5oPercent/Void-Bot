using Discord;
using Discord.WebSocket;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public interface ISlashCommand : IScope
{
    string Name => (string)Command.Name;

    string Description => (string)Command.Description;
    
    SlashCommandProperties Command { get; }
    
    Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default);
}

public interface IGlobalSlashCommand : ISlashCommand
{
}

public interface IGuildSlashCommand : ISlashCommand
{
    IEnumerable<ulong> GuildIds { get; }
}