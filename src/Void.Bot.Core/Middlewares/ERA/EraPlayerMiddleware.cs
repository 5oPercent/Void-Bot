using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Services.ERA;

namespace Void.Bot.Core.Middlewares.ERA;

public static class EraPlayerMiddleware
{
    public static void UseEraPlayer<TContext>(
        this IPipeConfigurator<TContext> configurator,
        IEraPlayerContext? playerContext = null)
        where TContext : IContext<IMessage>
    {
        if (playerContext == null && configurator.DependencyScope == null)
        {
            throw new DependencyScopeNotConfiguredException("ERA player middleware dependencies are not configured.");
        }

        playerContext ??= configurator.DependencyScope.Resolve<IEraPlayerContext>();
        configurator.AddPipeSpecification(new EraPlayerSpecification<TContext>(playerContext));
    }
}
