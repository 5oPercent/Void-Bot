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
    private readonly ILogger _logger;

    public DiscordSlashCommandRoute(IServiceScopeFactory scopeFactory, IEnumerable<ISlashCommand> commands, ILogger logger)
    {
        _scopeFactory = scopeFactory;
        _registerCommands = commands;
        _logger = logger;
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
        try
        {
            await RouteCoreAsync(callback).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "Discord slash command failed. Command: {CommandName}, GuildId: {GuildId}, UserId: {UserId}",
                callback.CommandName,
                callback.GuildId,
                callback.User.Id);

            await TryRespondWithErrorAsync(callback).ConfigureAwait(false);
        }
    }

    private async Task RouteCoreAsync(SocketSlashCommand callback)
    {
        using var scope = _scopeFactory.CreateScope();

        var allSlashCommands = scope.ServiceProvider.GetRequiredService<IEnumerable<ISlashCommand>>();
        var slashCommand = allSlashCommands.FirstOrDefault(x => x.Name == callback.CommandName);
        if (slashCommand == null)
        {
            await callback.RespondAsync("未知命令!", ephemeral: true).ConfigureAwait(false);
            return;
        }

        await callback.DeferAsync(slashCommand.Ephemeral).ConfigureAwait(false);

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
            var language = LanguageExtension.MapI18nCodeToEnum(callback.UserLocale);
            userProvider.SetCurrentUser(new CustomCurrentUser(user.Id, user.DisplayName, language));
        }

        _logger.Information("Discord Slash Command executed: {@Detail}", new
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

        await slashCommand.ExecuteAsync(callback).ConfigureAwait(false);
    }

    private static IEnumerable<object> ExtractSimplifiedOptions(IEnumerable<IApplicationCommandInteractionDataOption> options)
    {
        return options.Select(option => new
        {
            option.Name,
            Options = ExtractSimplifiedOptions(option.Options?.ToList() ?? []),
            Value = IsSensitiveOption(option.Name) ? "***" : SimplifyOptionValue(option.Value)
        });
    }

    private static object? SimplifyOptionValue(object? value)
    {
        return value switch
        {
            SocketGuildUser guildUser => new
            {
                guildUser.Id,
                guildUser.Username,
                guildUser.DisplayName
            },
            SocketUser socketUser => new
            {
                socketUser.Id,
                socketUser.Username
            },
            IUser discordUser => new
            {
                discordUser.Id,
                discordUser.Username
            },
            IChannel channel => new
            {
                channel.Id,
                channel.Name
            },
            IRole role => new
            {
                role.Id,
                role.Name
            },
            _ => value
        };
    }

    private static bool IsSensitiveOption(string name)
    {
        return name.Contains("password", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("密码", StringComparison.OrdinalIgnoreCase);
    }

    private async Task TryRespondWithErrorAsync(SocketSlashCommand callback)
    {
        try
        {
            if (callback.HasResponded)
            {
                await callback.ModifyOriginalResponseAsync(properties =>
                {
                    properties.Content = "指令执行失败，请稍后重试.";
                }).ConfigureAwait(false);
                return;
            }

            await callback.RespondAsync("指令执行失败，请稍后重试.", ephemeral: true).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex,
                "Failed to send Discord slash command error response. Command: {CommandName}, GuildId: {GuildId}, UserId: {UserId}",
                callback.CommandName,
                callback.GuildId,
                callback.User.Id);
        }
    }
}
