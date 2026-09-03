using Discord.WebSocket;

namespace Void.Bot.Core.Services.Discord.Component;

public abstract class DiscordComponentRouteBase : IDiscordComponentRoute
{
    protected abstract string CustomIdPrefix { get; }

    public bool ShouldExecute(string customId)
    {
        return customId.StartsWith(
            CustomIdPrefix,
            StringComparison.Ordinal);
    }

    public abstract Task ExecuteAsync(SocketMessageComponent callback);
}