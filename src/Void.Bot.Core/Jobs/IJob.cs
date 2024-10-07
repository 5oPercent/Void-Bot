using Void.Bot.Core.Services;

namespace Void.Bot.Core.Jobs;

public interface IJob : IScope
{
    string JobId { get; }
    
    Task Execute();
}

public interface IForgetJob : IJob
{
}

public interface IDelayedJob : IJob
{
    TimeSpan Delay { get; }
}

public interface IRecurringJob : IJob
{
    string CronExpression { get; }
}