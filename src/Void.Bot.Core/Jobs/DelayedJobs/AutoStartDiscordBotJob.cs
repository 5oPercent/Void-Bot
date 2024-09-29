using Mediator.Net;
using Serilog;
using Void.Bot.Core.Settings.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Jobs.DelayedJobs;

public class AutoStartDiscordBotJob : IDelayedJob
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly DiscordSetting _setting;
    
    public AutoStartDiscordBotJob(ILogger logger, IMediator mediator, DiscordSetting setting)
    {
        _logger = logger;
        _mediator = mediator;
        _setting = setting;
    }
    
    public string JobId => nameof(AutoStartDiscordBotJob);

    public TimeSpan Delay => TimeSpan.FromSeconds(3);

    public async Task Execute()
    {
        if (!_setting.AutoStart) return;
        
        _logger.Information("=== Auto start Discord bot ===");

        await _mediator.RequestAsync<DiscordStartBotRequest, DiscordStartBotResponse>(new DiscordStartBotRequest()).ConfigureAwait(false);
        
        _logger.Information("=== Auto start Discord bot done ===");
    }
}