using Void.Bot.Core.Services;

namespace Void.Bot.Core.Jobs;

public interface IJob : IScope
{
    string JobId { get; }
    
    Task Execute();
}