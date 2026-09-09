using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;

namespace Void.Bot.Core.Middlewares.TwoFactorAuthentication;

public static class TwoFactorAuthenticationMiddleware
{
    public static void UseTwoFactorAuthentication<TContext>(
        this IPipeConfigurator<TContext> configurator,
        ICurrentUser? currentUser = null,
        IAccountDataProvider? accountDataProvider = null)
        where TContext : IContext<IMessage>
    {
        if (configurator.DependencyScope == null)
        {
            throw new DependencyScopeNotConfiguredException(
                "Two-factor authentication middleware dependencies are not configured.");
        }

        currentUser ??= configurator.DependencyScope.Resolve<ICurrentUser>();
        accountDataProvider ??= configurator.DependencyScope.Resolve<IAccountDataProvider>();
        configurator.AddPipeSpecification(
            new TwoFactorAuthenticationSpecification<TContext>(currentUser, accountDataProvider));
    }
}
