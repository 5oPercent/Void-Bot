using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;

namespace Void.Bot.Core.Middlewares.UnitofWork;

public static class UnitOfWorkMiddleWare
{
    public static void UseUnitOfWork<TContext>(
        this IPipeConfigurator<TContext> configurator,
        IUnitOfWork? unitOfWork = null,
        DbContext? dbContext = null,
        IUnitOfWorkExecutionContext? executionContext = null)
        where TContext : IContext<IMessage>
    {
        if (configurator.DependencyScope == null && (unitOfWork == null || dbContext == null))
        {
            throw new DependencyScopeNotConfiguredException(
                $"{nameof(unitOfWork)} is not provided and IDependencyScope is not configured, Please ensure {nameof(unitOfWork)} is registered properly if you are using IoC container, otherwise please pass {nameof(unitOfWork)} as parameter");
        }

        unitOfWork ??= configurator.DependencyScope.Resolve<IUnitOfWork>();
        dbContext ??= configurator.DependencyScope.Resolve<DbContext>();
        executionContext ??= configurator.DependencyScope?.Resolve<IUnitOfWorkExecutionContext>()
                             ?? new UnitOfWorkExecutionContext();

        configurator.AddPipeSpecification(new UnitOfWorkSpecification<TContext>(
            unitOfWork, dbContext, executionContext));
    }
}
