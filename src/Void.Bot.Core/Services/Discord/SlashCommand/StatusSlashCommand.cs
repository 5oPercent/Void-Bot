using Discord;
using Discord.WebSocket;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public class StatusSlashCommand(DiscordSocketClient client) : IGlobalSlashCommand
{
    public bool ShouldDefer => false;

    public SlashCommandProperties Command => new SlashCommandBuilder()
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
    
    public async Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default)
    {
        var content = commandCallback.Data.Options.FirstOrDefault(o => o.Name == "内容")?.Value.ToString();
                
        var action = commandCallback.Data.Options.FirstOrDefault(o => o.Name == "动作")!.Value;
        var activity = (long)action switch
        {
            1 => new Game(content, ActivityType.Playing),
            2 => new Game(content, ActivityType.Listening),
            3 => new Game(content, ActivityType.Watching),
            _ => new Game(content)
        };
        await client.SetActivityAsync(activity).ConfigureAwait(false);
                
        var status = commandCallback.Data.Options.FirstOrDefault(o => o.Name == "状态")!.Value;
        var userStatus = (long)status switch
        {
            1 => UserStatus.Online,
            2 => UserStatus.AFK,
            3 => UserStatus.DoNotDisturb,
            _ => UserStatus.Idle
        };
        await client.SetStatusAsync(userStatus).ConfigureAwait(false);
                
        await commandCallback.RespondAsync("更改状态完成!", ephemeral: true).ConfigureAwait(false);
    }
}