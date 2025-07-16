using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Identity;

namespace Void.Bot.Core.Middlewares.Preload;

public static class LoadDestiny2PlayerMiddleware
{
    public static void UseLoadDestiny2Player<TContext>(this IPipeConfigurator<TContext> configurator, ICurrentUser? currentUser = null, Destiny2PlayerData? playerData = null, IBungieService? bungieService = null, IAccountDataProvider? accountDataProvider = null)
        where TContext : IContext<IMessage>
    {
        if (configurator.DependencyScope == null)
        {
            throw new DependencyScopeNotConfiguredException(
                $"{nameof(playerData)} is not provided and IDependencyScope is not configured, Please ensure {nameof(playerData)} is registered properly if you are using IoC container, otherwise please pass {nameof(playerData)} as parameter");
        }
        
        currentUser ??= configurator.DependencyScope.Resolve<ICurrentUser>();
        playerData ??= configurator.DependencyScope.Resolve<Destiny2PlayerData>();
        bungieService ??= configurator.DependencyScope.Resolve<IBungieService>();
        accountDataProvider ??= configurator.DependencyScope.Resolve<IAccountDataProvider>();
        
        configurator.AddPipeSpecification(new LoadDestiny2PlayerSpecification<TContext>(currentUser, playerData, bungieService, accountDataProvider));
    }
}