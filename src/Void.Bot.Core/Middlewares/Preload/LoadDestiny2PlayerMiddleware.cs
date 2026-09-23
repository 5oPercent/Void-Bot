using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Serilog;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.Bungie;

namespace Void.Bot.Core.Middlewares.Preload;

public static class LoadDestiny2PlayerMiddleware
{
    public static void UseLoadDestiny2Player<TContext>(
        this IPipeConfigurator<TContext> configurator,
        ILogger? logger = null,
        ICurrentUser? currentUser = null,
        Destiny2PlayerData? playerData = null,
        IBungieService? bungieService = null,
        IAccountDataProvider? accountDataProvider = null,
        ICacheManager? cacheManager = null,
        Destiny2Setting? destiny2Setting = null)
        where TContext : IContext<IMessage>
    {
        if ((logger == null || currentUser == null || playerData == null || bungieService == null ||
             accountDataProvider == null || cacheManager == null || destiny2Setting == null) &&
            configurator.DependencyScope == null)
        {
            throw new DependencyScopeNotConfiguredException(
                $"{nameof(playerData)} is not provided and IDependencyScope is not configured, Please ensure {nameof(playerData)} is registered properly if you are using IoC container, otherwise please pass {nameof(playerData)} as parameter");
        }
        
        logger ??= configurator.DependencyScope.Resolve<ILogger>();
        currentUser ??= configurator.DependencyScope.Resolve<ICurrentUser>();
        playerData ??= configurator.DependencyScope.Resolve<Destiny2PlayerData>();
        bungieService ??= configurator.DependencyScope.Resolve<IBungieService>();
        accountDataProvider ??= configurator.DependencyScope.Resolve<IAccountDataProvider>();
        cacheManager ??= configurator.DependencyScope.Resolve<ICacheManager>();
        destiny2Setting ??= configurator.DependencyScope.Resolve<Destiny2Setting>();
        
        configurator.AddPipeSpecification(new LoadDestiny2PlayerSpecification<TContext>(logger, currentUser, playerData, bungieService, accountDataProvider, cacheManager, destiny2Setting));
    }
}