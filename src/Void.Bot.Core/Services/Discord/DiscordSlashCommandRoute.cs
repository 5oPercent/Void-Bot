using Discord;
using Discord.WebSocket;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Discord.SlashCommand;

namespace Void.Bot.Core.Services.Discord;

public interface IDiscordSlashCommandRoute : ISingleton
{
    IEnumerable<SlashCommandProperties> GetGlobalCommands();
    
    IEnumerable<SlashCommandProperties> GetGuildCommands(List<ulong> guildIds);
    
    Task RouteAsync(SocketSlashCommand callback);
}

public class DiscordSlashCommandRoute : IDiscordSlashCommandRoute
{
    private readonly IEnumerable<ISlashCommand> _registerCommands;

    public DiscordSlashCommandRoute(IEnumerable<ISlashCommand> commands)
    {
        _registerCommands = commands;
    }

    public IEnumerable<SlashCommandProperties> GetGlobalCommands() => _registerCommands.OfType<IGlobalSlashCommand>().Select(x => x.Command).ToList();

    public IEnumerable<SlashCommandProperties> GetGuildCommands(List<ulong> guildIds)
    {
        var allGuildCommands = _registerCommands.OfType<IGuildSlashCommand>().ToList();
        if (allGuildCommands.IsNullOrEmpty())
            return new List<SlashCommandProperties>();

        if (guildIds.IsNullOrEmpty()) return allGuildCommands.Select(x => x.Command).ToList();
        
        var guildCommands = new List<SlashCommandProperties>();
        foreach (var guildId in guildIds)
        {
            var commands = allGuildCommands.Where(x => x.GuildIds.Contains(guildId) || x.GuildIds.IsNullOrEmpty()).Select(x => x.Command).ToList();
            if (!commands.IsNullOrEmpty())
            {
                guildCommands.AddRange(commands);
            }
        }
        
        return guildCommands;
    }

    public async Task RouteAsync(SocketSlashCommand callback)
    {
        var rc = _registerCommands.FirstOrDefault(x => x.Name == callback.CommandName);
        if (rc != null)
        {
            await rc.ExecuteAsync(callback);
        }
        else
        {
            await callback.RespondAsync("未知命令!", ephemeral: true).ConfigureAwait(false);
        }
    }
}