using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.EventAction;

[Table("event_action_execution")]
public class EventActionExecution : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();

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
