using System.ComponentModel.DataAnnotations;
using Void.Bot.Message.Constants.EventAction;

namespace Void.Bot.Message.Dto.EventAction;

public class EventActionRuleDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Event { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public int Sequence { get; set; }

    public List<EventActionConditionDto> Conditions { get; set; } = [];

    public List<ConfiguredEventActionDto> Actions { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

public class ConfiguredEventActionDto
{
    [Required]
    public string Type { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Version { get; set; } = 1;

    public bool Enabled { get; set; } = true;

    [Required]
    public string FailurePolicy { get; set; } = EventActionFailurePolicies.StopAll;

    [Required]
    public Dictionary<string, string[]> Arguments { get; set; } = [];
}

public class EventActionConditionDto
{
    [Required]
    public string Field { get; set; } = string.Empty;

    [Required]
    public string Operator { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string[] Values { get; set; } = [];
}

public class EventActionDefinitionData
{
    public List<BuiltInEventDefinitionDto> Events { get; set; } = [];

    public List<EventActionDefinitionDto> Actions { get; set; } = [];

    public List<EventActionArgumentOptionDto> FailurePolicies { get; set; } = [];

    public List<EventActionArgumentOptionDto> ExecutionStatuses { get; set; } = [];
}

public class BuiltInEventDefinitionDto
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<BuiltInEventFieldDefinitionDto> Fields { get; set; } = [];
}

public class BuiltInEventFieldDefinitionDto
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ValueType { get; set; } = "String";

    public bool Multiple { get; set; }

    public List<string> Operators { get; set; } = [];

    public List<EventActionArgumentOptionDto> Options { get; set; } = [];
}

public class EventActionDefinitionDto
{
    public string Type { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<string> SupportedEvents { get; set; } = [];

    public List<EventActionArgumentDefinitionDto> Arguments { get; set; } = [];
}

public class EventActionArgumentDefinitionDto
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ValueType { get; set; } = "String";

    public bool Required { get; set; }

    public bool Multiple { get; set; }

    public long? Minimum { get; set; }

    public long? Maximum { get; set; }

    public string? MaximumDependsOn { get; set; }

    public Dictionary<string, long> MaximumByOption { get; set; } = [];

    public int? MinLength { get; set; }

    public int? MaxLength { get; set; }

    public List<string> RequiredWith { get; set; } = [];

    public string ValueSource { get; set; } = string.Empty;

    public string DataSourceEndpoint { get; set; } = string.Empty;

    public string DataSourceValueField { get; set; } = string.Empty;

    public string DataSourceLabelField { get; set; } = string.Empty;

    public List<EventActionArgumentOptionDto> Options { get; set; } = [];
}

public class EventActionArgumentOptionDto
{
    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

public class EventActionExecutionDto
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public string Event { get; set; } = string.Empty;

    public DateTimeOffset EventOccurredAt { get; set; }

    public Guid RuleId { get; set; }

    public string RuleName { get; set; } = string.Empty;

    public int ActionIndex { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public int ActionVersion { get; set; }

    public string FailurePolicy { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset CompletedAt { get; set; }

    public long DurationMilliseconds { get; set; }

    public string? ErrorMessage { get; set; }
}
