using Serilog;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.EventAction;
using Void.Bot.Message.Constants.EventAction;
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

    int Version { get; }

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
    IEnumerable<IBuiltInEventDefinitionProvider> eventDefinitionProviders,
    IEnumerable<IConfiguredEventActionExecutor> executors,
    IRepository<EventActionExecution> executionRepository,
    IUnitOfWork unitOfWork,
    VoidBotDbContext dbContext,
    ILogger logger) : IBuiltInEventDispatcher
{
    public async Task DispatchAsync(IBuiltInEvent builtInEvent, CancellationToken cancellationToken)
    {
        var rules = await ruleService.GetEnabledRulesAsync(
            builtInEvent.Name, cancellationToken).ConfigureAwait(false);
        if (rules.Count == 0)
        {
            return;
        }

        var matchingEventDefinitionProviders = eventDefinitionProviders
            .Where(x => string.Equals(
                x.Definition.Name, builtInEvent.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (matchingEventDefinitionProviders.Count != 1)
        {
            throw new InvalidOperationException(
                $"Built-in event '{builtInEvent.Name}' must have exactly one definition provider, but found {matchingEventDefinitionProviders.Count}.");
        }

        var executorList = executors.ToList();
        foreach (var rule in rules)
        {
            if (!EventActionConditionEvaluator.Matches(
                    matchingEventDefinitionProviders[0], builtInEvent, rule.Conditions))
            {
                continue;
            }

            for (var actionIndex = 0; actionIndex < rule.Actions.Count; actionIndex++)
            {
                var action = rule.Actions[actionIndex];
                if (!action.Enabled)
                {
                    continue;
                }

                var startedAt = DateTimeOffset.UtcNow;
                var execution = new EventActionExecution
                {
                    EventId = builtInEvent.EventId,
                    Event = builtInEvent.Name,
                    EventOccurredAt = builtInEvent.OccurredAt,
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    ActionIndex = actionIndex,
                    ActionType = action.Type,
                    ActionVersion = action.Version,
                    FailurePolicy = action.FailurePolicy,
                    StartedAt = startedAt
                };

                try
                {
                    var matches = executorList
                        .Where(x => string.Equals(x.Type, action.Type, StringComparison.OrdinalIgnoreCase) &&
                                    x.Version == action.Version)
                        .ToList();
                    if (matches.Count != 1)
                    {
                        throw new InvalidOperationException(
                            $"Event action '{action.Type}' version {action.Version} must have exactly one executor, but found {matches.Count}.");
                    }

                    await matches[0].ExecuteAsync(
                        builtInEvent, action.Arguments, cancellationToken).ConfigureAwait(false);

                    execution.Status = EventActionExecutionStatuses.Succeeded;
                    execution.CompletedAt = DateTimeOffset.UtcNow;
                    execution.DurationMilliseconds = Math.Max(
                        0, (long)(execution.CompletedAt - execution.StartedAt).TotalMilliseconds);
                    await executionRepository.InsertAsync(
                        execution, cancellationToken).ConfigureAwait(false);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    dbContext.ChangeTracker.Clear();
                    var errorMessage = exception.Message;
                    execution.Status = EventActionExecutionStatuses.Failed;
                    execution.CompletedAt = DateTimeOffset.UtcNow;
                    execution.DurationMilliseconds = Math.Max(
                        0, (long)(execution.CompletedAt - execution.StartedAt).TotalMilliseconds);
                    execution.ErrorMessage = errorMessage.Length <= 2000
                        ? errorMessage
                        : errorMessage[..2000];
                    await executionRepository.InsertAsync(
                        execution, cancellationToken).ConfigureAwait(false);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    logger.Error(
                        exception,
                        "Failed to execute configured action {ActionType} version {ActionVersion} for event {EventName}, rule {RuleId}",
                        action.Type,
                        action.Version,
                        builtInEvent.Name,
                        rule.Id);

                    if (string.Equals(
                            action.FailurePolicy,
                            EventActionFailurePolicies.Continue,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (string.Equals(
                            action.FailurePolicy,
                            EventActionFailurePolicies.StopRule,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    return;
                }
            }
        }
    }
}
