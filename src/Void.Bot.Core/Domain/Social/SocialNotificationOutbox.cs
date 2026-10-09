using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Social;

[Table("social_notification_outbox")]
[Index(nameof(CompletedAt), nameof(NextAttemptAt))]
[Index(nameof(CreatedAt))]
public class SocialNotificationOutbox : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }
    public Guid RecipientAccountId { get; set; }
    public bool EraOnly { get; set; }
    [MaxLength(4096)]
    public string Payload { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public DateTimeOffset NextAttemptAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
