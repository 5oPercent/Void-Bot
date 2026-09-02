using Discord.WebSocket;
using Serilog;

namespace Void.Bot.Core.Services.Discord.Component;

public interface IDiscordComponentRoute : ISingleton
{
    bool ShouldExecute(string customId);

    Task ExecuteAsync(SocketMessageComponent callback);
}

public interface IDiscordComponentRouter : ISingleton
{
    Task ExecuteAsync(SocketMessageComponent callback);
}

public sealed class DiscordComponentRouter(
    IEnumerable<IDiscordComponentRoute> routes,
    ILogger logger) : IDiscordComponentRouter
{
    public async Task ExecuteAsync(SocketMessageComponent callback)
    {
        var route = routes.FirstOrDefault(x => x.ShouldExecute(callback.Data.CustomId));
        if (route == null)
        {
            await callback.RespondAsync("该操作已失效。", ephemeral: true).ConfigureAwait(false);
            return;
        }

        try
        {
            await route.ExecuteAsync(callback).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.Error(exception, "Discord component route failed. CustomId: {CustomId}, UserId: {UserId}", callback.Data.CustomId, callback.User.Id);
            if (!callback.HasResponded)
            {
                await callback.RespondAsync("操作失败，请稍后重试。", ephemeral: true).ConfigureAwait(false);
            }
        }
    }
}