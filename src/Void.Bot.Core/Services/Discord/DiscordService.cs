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

    public async Task InitializationSlashCommandAsync(CancellationToken cancellationToken)
    {
        var globalCommand = new SlashCommandBuilder();
        globalCommand.WithName("嗨!");
        globalCommand.WithDescription("打个招呼吧！");
        
    }
}