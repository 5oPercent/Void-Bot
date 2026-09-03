using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Void.Bot.Core.Resources.i18n.Discord;
using Void.Bot.Core.Services.Language;

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
    ILogger logger,
    IServiceScopeFactory scopeFactory) : IDiscordComponentRouter
{
    public async Task ExecuteAsync(SocketMessageComponent callback)
    {
        var route = routes.FirstOrDefault(x => x.ShouldExecute(callback.Data.CustomId));
        if (route == null)
        {
            using var scope = scopeFactory.CreateScope();
            var localizationService = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
            await callback.RespondAsync(localizationService.GetValue<DiscordContextTemplate>(x => x.ComponentExpired, callback.UserLocale), ephemeral: true).ConfigureAwait(false);
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
                using var scope = scopeFactory.CreateScope();
                var localizationService = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
                await callback.RespondAsync(localizationService.GetValue<DiscordContextTemplate>(x => x.ComponentFailed, callback.UserLocale), ephemeral: true).ConfigureAwait(false);
            }
        }
    }
}
