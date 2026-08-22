using System.ComponentModel.DataAnnotations;

namespace Void.Bot.Message.Dto.EventAction;

public class EventActionRuleDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Event { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public int Sequence { get; set; }

    public List<ConfiguredEventActionDto> Actions { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

public class ConfiguredEventActionDto
{
    [Required]
    public string Type { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    [Required]
    public Dictionary<string, string[]> Arguments { get; set; } = [];
}

public class EventActionDefinitionData
{
    public List<BuiltInEventDefinitionDto> Events { get; set; } = [];

    public List<EventActionDefinitionDto> Actions { get; set; } = [];
}

public class BuiltInEventDefinitionDto
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public class EventActionDefinitionDto
{
    public string Type { get; set; } = string.Empty;

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

    public bool Required { get; set; }

    public bool Multiple { get; set; }

    public string ValueSource { get; set; } = string.Empty;

    public string DataSourceEndpoint { get; set; } = string.Empty;

    public string DataSourceValueField { get; set; } = string.Empty;

    public string DataSourceLabelField { get; set; } = string.Empty;
}
