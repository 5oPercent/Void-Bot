namespace Void.Bot.Core.Jobs;

public interface IRecurringJob : IJob
{
    string CronExpression { get; }
}