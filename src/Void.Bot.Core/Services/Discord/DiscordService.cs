using Discord;
using Discord.WebSocket;
using Serilog;
using Void.Bot.Core.Settings.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Services.Discord;

public interface IDiscordService : IScope
{
    Task<DiscordStartBotResponse> StartAsync(DiscordStartBotRequest request, CancellationToken cancellationToken);
    
    Task<DiscordStopBotResponse> StopAsync(DiscordStopBotRequest request, CancellationToken cancellationToken);
    
    Task ApplyGlobalSlashCommandAsync(DiscordApplyGlobalSlashCommandRequest request, CancellationToken cancellationToken);

    Task ApplyGuildSlashCommandAsync(DiscordApplyGuildSlashCommandRequest request, CancellationToken cancellationToken);
}

public class DiscordService : IDiscordService
{
    private readonly ILogger _logger;
    private readonly DiscordSetting _discordSetting;
    private readonly IDiscordDataProvider _discordDataProvider;
    
    private static DiscordSocketClient _client;

    public DiscordService(ILogger logger, DiscordSetting discordSetting, IDiscordDataProvider discordDataProvider, DiscordSocketClient client)
    {
        _logger = logger;
        _discordSetting = discordSetting;
        _discordDataProvider = discordDataProvider;
        
        _client = client;
        
        _client.SlashCommandExecuted += SlashCommandHandlerAsync;
    }

    public async Task<DiscordStartBotResponse> StartAsync(DiscordStartBotRequest request, CancellationToken cancellationToken)
    {
        await _client.LoginAsync(TokenType.Bot, _discordSetting.Token).ConfigureAwait(false);
        await _client.StartAsync().ConfigureAwait(false);

        return new DiscordStartBotResponse();
    }

    public async Task<DiscordStopBotResponse> StopAsync(DiscordStopBotRequest request, CancellationToken cancellationToken)
    {
        if (_client.ConnectionState != ConnectionState.Disconnected)
        {
            await _client.StopAsync().ConfigureAwait(false);
        }

        return new DiscordStopBotResponse();
    }

    public async Task ApplyGlobalSlashCommandAsync(DiscordApplyGlobalSlashCommandRequest request ,CancellationToken cancellationToken)
    {
        var properties = new List<ApplicationCommandProperties>()
        {
            new SlashCommandBuilder()
                .WithName("嗨")
                .WithDescription("打个招呼吧!").Build(),
        };

        
        await _client.BulkOverwriteGlobalApplicationCommandsAsync(properties.ToArray()).ConfigureAwait(false);
    }
    
    public async Task ApplyGuildSlashCommandAsync(DiscordApplyGuildSlashCommandRequest request, CancellationToken cancellationToken)
    {
        var guild = _client.GetGuild(request.GuildId);
        if (guild == null) return;

        var statusCommand = new SlashCommandBuilder()
            .WithName("状态")
            .WithDescription("更改机器人线上状态.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("动作")
                .WithDescription("选择一个动作状态.")
                .WithRequired(true)
                .AddChoice("正在玩", 1)
                .AddChoice("正在听", 2)
                .AddChoice("正在看", 3)
                .WithType(ApplicationCommandOptionType.Integer))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("内容")
                .WithDescription("选择一个内容.")
                .WithRequired(true)
                .AddChoice("虚空", "虚空")
                .AddChoice("光能", "光能")
                .AddChoice("暗影", "暗影")
                .AddChoice("守护者", "守护者")
                .AddChoice("三角舰队", "三角舰队")
                .AddChoice("萨瓦图恩", "萨瓦图恩")
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("状态")
                .WithDescription("选择一个线上状态.")
                .WithRequired(true)
                .AddChoice("绿色 在线", 1)
                .AddChoice("黄色 离开", 2)
                .AddChoice("红色 勿扰", 3)
                .WithType(ApplicationCommandOptionType.Integer))
            .Build();
        
        await guild.CreateApplicationCommandAsync(statusCommand).ConfigureAwait(false);

    }

    private static async Task SlashCommandHandlerAsync(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "嗨":
                await command.RespondAsync("你好!").ConfigureAwait(false);
                break;
            
            case "状态":
                var content = command.Data.Options.FirstOrDefault(o => o.Name == "内容")?.Value.ToString();
                
                var action = command.Data.Options.FirstOrDefault(o => o.Name == "动作")!.Value;
                var activity = (long)action switch
                {
                    1 => new Game(content, ActivityType.Playing),
                    2 => new Game(content, ActivityType.Listening),
                    3 => new Game(content, ActivityType.Watching),
                    _ => new Game(content)
                };
                await _client.SetActivityAsync(activity).ConfigureAwait(false);
                
                var status = command.Data.Options.FirstOrDefault(o => o.Name == "状态")!.Value;
                var userStatus = (long)status switch
                {
                    1 => UserStatus.Online,
                    2 => UserStatus.AFK,
                    3 => UserStatus.DoNotDisturb,
                    _ => UserStatus.Idle
                };
                await _client.SetStatusAsync(userStatus).ConfigureAwait(false);
                
                await command.RespondAsync("更改状态完成!").ConfigureAwait(false);
                break;
            default:
                await command.RespondAsync("未知命令!").ConfigureAwait(false);
                break;
        }
    }
}