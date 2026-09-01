using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Core.Services.Identity;

namespace Void.Bot.Core.Middlewares.DiscordBinding;

public static class DiscordBindingMiddleware
{
    public static void UseDiscordBinding<TContext>(
        this IPipeConfigurator<TContext> configurator,
        ICurrentUser? currentUser = null,
        IAccountDataProvider? accountDataProvider = null,
        IDiscordBindingContext? bindingContext = null)
        where TContext : IContext<IMessage>
    {
        if (configurator.DependencyScope == null &&
            (currentUser == null || accountDataProvider == null || bindingContext == null))
        {
            throw new DependencyScopeNotConfiguredException(
                "Discord binding middleware dependencies are not configured.");
        }

        currentUser ??= configurator.DependencyScope.Resolve<ICurrentUser>();
        accountDataProvider ??= configurator.DependencyScope.Resolve<IAccountDataProvider>();
        bindingContext ??= configurator.DependencyScope.Resolve<IDiscordBindingContext>();

        configurator.AddPipeSpecification(new DiscordBindingSpecification<TContext>(
            currentUser, accountDataProvider, bindingContext));
    }
}
