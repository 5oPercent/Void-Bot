using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.EventAction;
using Void.Bot.Message.Commands.EventAction;
using Void.Bot.Message.Constants.EventAction;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Request.EventAction;

namespace Void.Bot.Core.Services.EventAction;

public interface IEventActionRuleService : IScope
{
    Task<GetEventActionDefinitionResponse> GetDefinitionsAsync(CancellationToken cancellationToken);

    Task<GetEventActionRuleListResponse> GetRuleListAsync(
        GetEventActionRuleListRequest request, CancellationToken cancellationToken);

    Task<List<EventActionRuleDto>> GetEnabledRulesAsync(
        string eventName, CancellationToken cancellationToken);

    Task<AddEventActionRuleResponse> AddRuleAsync(
        AddEventActionRuleCommand command, CancellationToken cancellationToken);

    Task<UpdateEventActionRuleResponse> UpdateRuleAsync(
        UpdateEventActionRuleCommand command, CancellationToken cancellationToken);

    Task<DeleteEventActionRuleResponse> DeleteRulesAsync(
        DeleteEventActionRuleCommand command, CancellationToken cancellationToken);
}

public class EventActionRuleService(
    IRepository<EventActionRule> ruleRepository,
    IEnumerable<IBuiltInEventDefinitionProvider> eventDefinitionProviders,
    IEnumerable<IConfiguredEventActionExecutor> executors) : IEventActionRuleService
{
    private readonly List<IBuiltInEventDefinitionProvider> _eventDefinitionProviders =
        eventDefinitionProviders.ToList();

    private readonly List<BuiltInEventDefinitionDto> _eventDefinitions = eventDefinitionProviders
        .Select(x => x.Definition)
        .OrderBy(x => x.Name)
        .ToList();

    public Task<GetEventActionDefinitionResponse> GetDefinitionsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new GetEventActionDefinitionResponse
        {
            Data = new EventActionDefinitionData
            {
                Events = _eventDefinitions,
                Actions = executors.Select(x => x.Definition)
                    .OrderBy(x => x.Type)
                    .ThenBy(x => x.Version)
                    .ToList(),
                FailurePolicies =
                [
                    new EventActionArgumentOptionDto
                    {
                        Value = EventActionFailurePolicies.StopAll,
                        Label = "停止全部"
                    },
                    new EventActionArgumentOptionDto
                    {
                        Value = EventActionFailurePolicies.StopRule,
                        Label = "停止当前规则"
                    },
                    new EventActionArgumentOptionDto
                    {
                        Value = EventActionFailurePolicies.Continue,
                        Label = "继续执行"
                    }
                ],
                ExecutionStatuses =
                [
                    new EventActionArgumentOptionDto
                    {
                        Value = EventActionExecutionStatuses.Succeeded,
                        Label = "成功"
                    },
                    new EventActionArgumentOptionDto
                    {
                        Value = EventActionExecutionStatuses.Failed,
                        Label = "失败"
                    }
                ]
            }
        });
    }

    public async Task<GetEventActionRuleListResponse> GetRuleListAsync(
        GetEventActionRuleListRequest request, CancellationToken cancellationToken)
    {
        var query = ruleRepository.QueryNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Event))
        {
            var eventName = _eventDefinitions
                .FirstOrDefault(x => string.Equals(x.Name, request.Event, StringComparison.OrdinalIgnoreCase))?.Name
                ?? request.Event;
            query = query.Where(x => x.Event == eventName);
        }

        var rules = await query.OrderBy(x => x.Sequence)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new GetEventActionRuleListResponse
        {
            Data = rules.Select(MapRule).ToList()
        };
    }

    public async Task<List<EventActionRuleDto>> GetEnabledRulesAsync(
        string eventName, CancellationToken cancellationToken)
    {
        var rules = await ruleRepository.QueryNoTracking()
            .Where(x => x.Enabled && x.Event == eventName)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return rules.Select(MapRule).ToList();
    }

    public async Task<AddEventActionRuleResponse> AddRuleAsync(
        AddEventActionRuleCommand command, CancellationToken cancellationToken)
    {
        var response = new AddEventActionRuleResponse();
        var validationMessage = await ValidateRuleAsync(
            command.Event, command.Conditions, command.Actions, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationMessage))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = validationMessage;
            return response;
        }

        var rule = new EventActionRule
        {
            Name = command.Name.Trim(),
            Event = _eventDefinitions.First(x =>
                string.Equals(x.Name, command.Event, StringComparison.OrdinalIgnoreCase)).Name,
            Enabled = command.Enabled,
            Sequence = command.Sequence,
            ConditionsJson = JsonSerializer.Serialize(command.Conditions),
            ActionsJson = JsonSerializer.Serialize(command.Actions),
            CreatedAt = DateTimeOffset.UtcNow
        };
        await ruleRepository.InsertAsync(rule, cancellationToken).ConfigureAwait(false);

        response.Data = MapRule(rule);
        return response;
    }

    public async Task<UpdateEventActionRuleResponse> UpdateRuleAsync(
        UpdateEventActionRuleCommand command, CancellationToken cancellationToken)
    {
        var response = new UpdateEventActionRuleResponse();
        var rule = await ruleRepository.SingleOrDefaultAsync(
            x => x.Id == command.Id, cancellationToken).ConfigureAwait(false);
        if (rule == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Event action rule not found.";
            return response;
        }

        var validationMessage = await ValidateRuleAsync(
            command.Event, command.Conditions, command.Actions, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationMessage))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = validationMessage;
            return response;
        }

        rule.Name = command.Name.Trim();
        rule.Event = _eventDefinitions.First(x =>
            string.Equals(x.Name, command.Event, StringComparison.OrdinalIgnoreCase)).Name;
        rule.Enabled = command.Enabled;
        rule.Sequence = command.Sequence;
        rule.ConditionsJson = JsonSerializer.Serialize(command.Conditions);
        rule.ActionsJson = JsonSerializer.Serialize(command.Actions);
        rule.UpdatedAt = DateTimeOffset.UtcNow;
        await ruleRepository.UpdateAsync(rule, cancellationToken).ConfigureAwait(false);

        response.Data = MapRule(rule);
        return response;
    }

    public async Task<DeleteEventActionRuleResponse> DeleteRulesAsync(
        DeleteEventActionRuleCommand command, CancellationToken cancellationToken)
    {
        await ruleRepository.DeleteAsync(
            x => command.IdList.Contains(x.Id), cancellationToken).ConfigureAwait(false);
        return new DeleteEventActionRuleResponse();
    }

    private async Task<string?> ValidateRuleAsync(
        string eventName,
        List<EventActionConditionDto> conditions,
        List<ConfiguredEventActionDto> actions,
        CancellationToken cancellationToken)
    {
        var matchingEventDefinitions = _eventDefinitions
            .Where(x => string.Equals(x.Name, eventName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (matchingEventDefinitions.Count == 0)
        {
            return $"Built-in event '{eventName}' is not supported.";
        }

        if (matchingEventDefinitions.Count != 1)
        {
            return $"Built-in event '{eventName}' must have exactly one definition provider, but found {matchingEventDefinitions.Count}.";
        }

        var matchingEventDefinitionProviders = _eventDefinitionProviders
            .Where(x => string.Equals(
                x.Definition.Name, matchingEventDefinitions[0].Name, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (matchingEventDefinitionProviders.Count != 1)
        {
            return $"Built-in event '{eventName}' must have exactly one definition provider, but found {matchingEventDefinitionProviders.Count}.";
        }

        var conditionValidationMessage = EventActionConditionEvaluator.Validate(
            matchingEventDefinitions[0], conditions);
        if (!string.IsNullOrEmpty(conditionValidationMessage))
        {
            return conditionValidationMessage;
        }

        if (actions.Count == 0)
        {
            return "At least one event action is required.";
        }

        var executorList = executors.ToList();
        foreach (var action in actions)
        {
            if (action == null)
            {
                return "Event action cannot be null.";
            }

            if (!EventActionFailurePolicies.All.Contains(
                    action.FailurePolicy, StringComparer.OrdinalIgnoreCase))
            {
                return $"Event action '{action.Type}' failure policy must be StopAll, StopRule, or Continue.";
            }

            var matches = executorList
                .Where(x => string.Equals(x.Type, action.Type, StringComparison.OrdinalIgnoreCase) &&
                            x.Version == action.Version)
                .ToList();
            if (matches.Count != 1)
            {
                return $"Event action '{action.Type}' version {action.Version} must have exactly one executor, but found {matches.Count}.";
            }

            var validationMessage = await matches[0].ValidateAsync(
                eventName, action.Arguments, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                return validationMessage;
            }
        }

        return null;
    }

    private static EventActionRuleDto MapRule(EventActionRule rule)
    {
        return new EventActionRuleDto
        {
            Id = rule.Id,
            Name = rule.Name,
            Event = rule.Event,
            Enabled = rule.Enabled,
            Sequence = rule.Sequence,
            Conditions = JsonSerializer.Deserialize<List<EventActionConditionDto>>(rule.ConditionsJson) ?? [],
            Actions = JsonSerializer.Deserialize<List<ConfiguredEventActionDto>>(rule.ActionsJson) ?? [],
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }
}
