using Serilog;

namespace Void.Bot.Core.Services.Steam;

public interface ISteamService : IScope
{
}

public class SteamService : ISteamService
{
    private readonly ILogger _logger;
    
    public SteamService(ILogger logger)
    {
        _logger = logger;
    }
}