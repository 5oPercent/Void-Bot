using Serilog;

namespace Void.Bot.Core.Services.Discord;

public interface IDiscordService : IScope
{
}

public class DiscordService : IDiscordService
{
    private readonly ILogger _logger;
    
    public DiscordService(ILogger logger)
    {
        _logger = logger;
    }
}