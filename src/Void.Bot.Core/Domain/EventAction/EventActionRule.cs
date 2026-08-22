using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.EventAction;

[Table("event_action_rule")]
public class EventActionRule : IEntityAudit<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Event { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public int Sequence { get; set; }

    public string ActionsJson { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
