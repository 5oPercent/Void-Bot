using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.EventAction;
using Void.Bot.Message.Commands.EventAction;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Events.EventAction;
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
    IEnumerable<IConfiguredEventActionExecutor> executors) : IEventActionRuleService
{
    private static readonly List<BuiltInEventDefinitionDto> EventDefinitions =
    [
        new BuiltInEventDefinitionDto
        {
            Name = BuiltInEventNames.AccountRegistered,
            DisplayName = "账号注册成功",
            Description = "Void 账号事务提交成功后执行配置动作。"
        }
    ];

    public Task<GetEventActionDefinitionResponse> GetDefinitionsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new GetEventActionDefinitionResponse
        {
            Data = new EventActionDefinitionData
            {
                Events = EventDefinitions,
                Actions = executors.Select(x => x.Definition).OrderBy(x => x.Type).ToList()
            }
        });
    }

    public async Task<GetEventActionRuleListResponse> GetRuleListAsync(
        GetEventActionRuleListRequest request, CancellationToken cancellationToken)
    {
        var query = ruleRepository.QueryNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Event))
        {
            var eventName = EventDefinitions
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
            command.Event, command.Actions, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationMessage))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = validationMessage;
            return response;
        }

        var rule = new EventActionRule
        {
            Event = EventDefinitions.First(x =>
                string.Equals(x.Name, command.Event, StringComparison.OrdinalIgnoreCase)).Name,
            Enabled = command.Enabled,
            Sequence = command.Sequence,
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
            command.Event, command.Actions, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationMessage))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = validationMessage;
            return response;
        }

        rule.Event = EventDefinitions.First(x =>
            string.Equals(x.Name, command.Event, StringComparison.OrdinalIgnoreCase)).Name;
        rule.Enabled = command.Enabled;
        rule.Sequence = command.Sequence;
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
        List<ConfiguredEventActionDto> actions,
        CancellationToken cancellationToken)
    {
        if (!EventDefinitions.Any(x => string.Equals(x.Name, eventName, StringComparison.OrdinalIgnoreCase)))
        {
            return $"Built-in event '{eventName}' is not supported.";
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

            var matches = executorList
                .Where(x => string.Equals(x.Type, action.Type, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matches.Count != 1)
            {
                return $"Event action '{action.Type}' must have exactly one executor, but found {matches.Count}.";
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
            Event = rule.Event,
            Enabled = rule.Enabled,
            Sequence = rule.Sequence,
            Actions = JsonSerializer.Deserialize<List<ConfiguredEventActionDto>>(rule.ActionsJson) ?? [],
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }
}
