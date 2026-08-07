using Serilog;
using Void.Bot.Core.Services.Discord;

namespace Void.Bot.Core.Jobs.RecurringJobs;

public class CleanupDiscordTemporaryVoiceChannelJob : IRecurringJob
{
    private readonly ILogger _logger;
    private readonly IDiscordVoiceChannelService _service;

    public CleanupDiscordTemporaryVoiceChannelJob(ILogger logger, IDiscordVoiceChannelService service)
    {
        _logger = logger;
        _service = service;
    }

    public string JobId => nameof(CleanupDiscordTemporaryVoiceChannelJob);

    public string CronExpression => "* * * * *";

    public async Task Execute()
    {
        _logger.Debug("=== Cleanup Discord temporary voice channels ===");
        await _service.CleanupExpiredEmptyChannelsAsync().ConfigureAwait(false);
    }
}
