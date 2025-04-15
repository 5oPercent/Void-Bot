using System.Linq.Expressions;
using Hangfire;
using Hangfire.States;

namespace Void.Bot.Core.Services.Jobs;

public interface IVoidBotBackgroundJobClient : IScope
{
    string Enqueue(Expression<Func<Task>> methodCall, string queue = "default");
    
    string Enqueue<T>(Expression<Func<T, Task>> methodCall, string queue = "default");
    
    string ContinueJobWith(string parentJobId, Expression<Func<Task>> methodCall, string queue = "default");
    
    string ContinueJobWith<T>(string parentJobId, Expression<Func<T,Task>> methodCall, string queue = "default");
}

public class VoidBotBackgroundJobClient : IVoidBotBackgroundJobClient
{
    private readonly Func<IBackgroundJobClient> _backgroundJobClientFunc;

    public VoidBotBackgroundJobClient(Func<IBackgroundJobClient> backgroundJobClientFunc)
    {
        _backgroundJobClientFunc = backgroundJobClientFunc;
    }

    public string Enqueue(Expression<Func<Task>> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc().Create(methodCall, new EnqueuedState(queue));
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc().Create(methodCall, new EnqueuedState(queue));
    }

    public string ContinueJobWith(string parentJobId, Expression<Func<Task>> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc().ContinueJobWith(parentJobId, methodCall, new EnqueuedState(queue));
    }
    
    public string ContinueJobWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc().ContinueJobWith(parentJobId, methodCall, new EnqueuedState(queue));
    }
}