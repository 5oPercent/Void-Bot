using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Social;

namespace Void.Bot.Core.Domain.Social;

[Table("friend_request")]
[Index(nameof(SenderAccountId), nameof(RequestId), IsUnique = true)]
[Index(nameof(RecipientAccountId), nameof(Status), nameof(CreatedAt))]
public class FriendRequest : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }
    public Guid SenderAccountId { get; set; }
    public Guid RecipientAccountId { get; set; }
    public Guid RequestId { get; set; }
    public FriendRequestStatus Status { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public long Version { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
}
