using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Discord.SlashCommand;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Services.Discord;

public interface IDiscordSlashCommandRoute : ISingleton
{
    IEnumerable<SlashCommandProperties> GetGlobalCommands();
    
    IEnumerable<SlashCommandProperties> GetGuildCommands(List<ulong> guildIds);
    
    Task RouteAsync(SocketSlashCommand callback);
}

public class DiscordSlashCommandRoute : IDiscordSlashCommandRoute
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEnumerable<ISlashCommand> _registerCommands;

    public DiscordSlashCommandRoute(IServiceScopeFactory scopeFactory, IEnumerable<ISlashCommand> commands)
    {
        _scopeFactory = scopeFactory;
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
        await callback.DeferAsync().ConfigureAwait(false);
        
        using var scope = _scopeFactory.CreateScope();
        
        var allSlashCommands = scope.ServiceProvider.GetRequiredService<IEnumerable<ISlashCommand>>();
        var sc = allSlashCommands.FirstOrDefault(x => x.Name == callback.CommandName);
        if (sc == null)
        {
            await callback.RespondAsync("未知命令!", ephemeral: true).ConfigureAwait(false);
            return;
        }
        
        var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
        var userResponse = await accountService.GetUserAccountByThirdPartyAsync(
            new GetUserAccountByThirdPartyRequest
            {
                Platform = AppPlatformEnum.Discord,
                ThirdPartyUserId = callback.User.Id.ToString()
            }, CancellationToken.None).ConfigureAwait(false);
        
        var user = userResponse.Data;
        if (user != null)
        {
            var userProvider = scope.ServiceProvider.GetRequiredService<ICurrentUserProvider>();
            userProvider.SetCurrentUser(new CustomCurrentUser(user.Id, user.DisplayName, user.Level, user.Source));
        }
        
        Log.Information("Discord Slash Command executed: {@Detail}", new
        {
            SlashCommand = new
            {
                Name = callback.CommandName,
                Options = ExtractSimplifiedOptions(callback.Data.Options)
            },
            User = new {
                Void = new
                {
                    user?.Id,
                    user?.DisplayName
                },
                Discord = new
                {
                    Id = callback.User.Id.ToString(),
                    callback.User.GlobalName,
                    callback.User.Username
                }
            }
        });
        
        await sc.ExecuteAsync(callback);
    }
    
    private static IEnumerable<object> ExtractSimplifiedOptions(IEnumerable<IApplicationCommandInteractionDataOption> options)
    {
        return options.Select(o => new 
        {
            o.Name,
            Options = ExtractSimplifiedOptions(o.Options?.ToList() ?? []),
            o.Value
        });
    }
}