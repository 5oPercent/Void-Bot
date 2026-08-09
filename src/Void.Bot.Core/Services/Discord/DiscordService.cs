using Discord;
using Discord.WebSocket;
using Serilog;
using Void.Bot.Core.Services.Discord.DataProvider;
using Void.Bot.Core.Services.Http;
using Void.Bot.Core.Settings.Discord;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Services.Discord;

public partial interface IDiscordService : IScope
{
    Task<DiscordStartBotResponse> StartAsync(DiscordStartBotRequest request, CancellationToken cancellationToken);
    
    Task<DiscordStopBotResponse> StopAsync(DiscordStopBotRequest request, CancellationToken cancellationToken);
    
    Task ApplyGlobalSlashCommandAsync(DiscordApplyGlobalSlashCommandRequest request, CancellationToken cancellationToken);

    Task ApplyGuildSlashCommandAsync(DiscordApplyGuildSlashCommandRequest request, CancellationToken cancellationToken);
}

public partial class DiscordService : IDiscordService
{
    private readonly DiscordSetting _setting;
    private readonly OAuthPlatformSetting _oauthSetting;
    private readonly IDiscordDataProvider _dataProvider;
    private readonly IDiscordSlashCommandRoute _slashCommandRoute;
    private readonly IVoidBotHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    
    private readonly DiscordSocketClient _client;

    public DiscordService(
        DiscordSetting setting,
        OAuthSetting oauthSetting,
        IDiscordDataProvider dataProvider,
        IDiscordSlashCommandRoute slashCommandRoute,
        IVoidBotHttpClientFactory httpClientFactory,
        ILogger logger,
        DiscordSocketClient client)
    {
        _setting = setting;
        _oauthSetting = oauthSetting.Discord;
        _dataProvider = dataProvider;
        _slashCommandRoute = slashCommandRoute; 
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _client = client;
    }

    public async Task<DiscordStartBotResponse> StartAsync(DiscordStartBotRequest request, CancellationToken cancellationToken)
    {
        await _client.LoginAsync(TokenType.Bot, _setting.Token).ConfigureAwait(false);
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
        var commands = _slashCommandRoute.GetGlobalCommands().Cast<ApplicationCommandProperties>().ToArray();
        var requestOptions = new RequestOptions { CancelToken = cancellationToken };

        await _client.BulkOverwriteGlobalApplicationCommandsAsync(commands, requestOptions).ConfigureAwait(false);
    }
    
    public async Task ApplyGuildSlashCommandAsync(DiscordApplyGuildSlashCommandRequest request, CancellationToken cancellationToken)
    {
        var guild = _client.GetGuild(request.GuildId);
        if (guild == null) return;

        var commands = _slashCommandRoute.GetGuildCommands([request.GuildId]).Cast<ApplicationCommandProperties>().ToArray();
        var requestOptions = new RequestOptions { CancelToken = cancellationToken };

        await guild.BulkOverwriteApplicationCommandAsync(commands, requestOptions).ConfigureAwait(false);
    }
}
