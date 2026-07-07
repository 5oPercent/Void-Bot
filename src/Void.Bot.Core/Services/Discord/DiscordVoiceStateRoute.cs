using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Void.Bot.Core.Services.Discord;

public interface IDiscordVoiceStateRoute : ISingleton
{
    Task RouteAsync(SocketUser user, SocketVoiceState before, SocketVoiceState after);
}

public class DiscordVoiceStateRoute : IDiscordVoiceStateRoute
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    public DiscordVoiceStateRoute(IServiceScopeFactory scopeFactory, ILogger logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task RouteAsync(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDiscordVoiceChannelService>();
            await service.HandleVoiceStateUpdatedAsync(user, before, after).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Discord voice state route failed for user {UserId}", user.Id);
        }
    }
}
