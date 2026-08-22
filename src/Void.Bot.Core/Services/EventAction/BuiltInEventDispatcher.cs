using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Services.EventAction;

public interface IBuiltInEventDispatcher : IScope
{
    Task DispatchAsync(IBuiltInEvent builtInEvent, CancellationToken cancellationToken);
}

public interface IConfiguredEventActionExecutor : IScope
{
    string Type { get; }

    EventActionDefinitionDto Definition { get; }

    Task<string?> ValidateAsync(
        string eventName,
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken);

    Task ExecuteAsync(
        IBuiltInEvent builtInEvent,
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken);
}

public class BuiltInEventDispatcher(
    IEventActionRuleService ruleService,
    IEnumerable<IConfiguredEventActionExecutor> executors) : IBuiltInEventDispatcher
{
    public async Task DispatchAsync(IBuiltInEvent builtInEvent, CancellationToken cancellationToken)
    {
        var rules = await ruleService.GetEnabledRulesAsync(
            builtInEvent.Name, cancellationToken).ConfigureAwait(false);
        var actions = rules
            .SelectMany(x => x.Actions)
            .Where(x => x.Enabled)
            .ToList();

        if (actions.Count == 0)
        {
            return;
        }

        var executorList = executors.ToList();
        foreach (var action in actions)
        {
            var matches = executorList
                .Where(x => string.Equals(x.Type, action.Type, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Event action '{action.Type}' must have exactly one executor, but found {matches.Count}.");
            }

            await matches[0].ExecuteAsync(
                builtInEvent, action.Arguments, cancellationToken).ConfigureAwait(false);
        }
    }
}
