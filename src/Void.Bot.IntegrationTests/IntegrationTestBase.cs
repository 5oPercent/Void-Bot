using Autofac;
using Void.Bot.Core.Data;

namespace Void.Bot.IntegrationTest;

public class IntegrationTestBase
{
    private ILifetimeScope _lifetimeScope;

    protected IntegrationTestBase(ILifetimeScope lifetimeScope = null)
    {
        _lifetimeScope = lifetimeScope;
    }
    
    protected void SetupScope(ILifetimeScope lifetimeScope) => _lifetimeScope = lifetimeScope;

    protected async Task Run<T>(Func<T, Task> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var dependency = extraRegistration != null
            ? _lifetimeScope.BeginLifetimeScope(extraRegistration).Resolve<T>()
            : _lifetimeScope.BeginLifetimeScope().Resolve<T>();
        
        await action(dependency);
    }
    
    protected Task Run<T, U>(Func<T, U, Task> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var lifetime = extraRegistration != null
            ? _lifetimeScope.BeginLifetimeScope(extraRegistration)
            : _lifetimeScope.BeginLifetimeScope();
        
        var dependency = lifetime.Resolve<T>();
        var dependency2 = lifetime.Resolve<U>();
        
        return action(dependency, dependency2);
    }
    
    protected async Task RunWithUnitOfWork<T>(Func<T, Task> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var scope = extraRegistration != null
            ? _lifetimeScope.BeginLifetimeScope(extraRegistration)
            : _lifetimeScope.BeginLifetimeScope();

        var dependency = scope.Resolve<T>();
        var unitOfWork = scope.Resolve<IUnitOfWork>();
        
        await action(dependency);
        await unitOfWork.SaveChangesAsync();
    }
}