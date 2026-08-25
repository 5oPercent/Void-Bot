using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Core.Domain.Destiny2;

[Table("destiny2_pgcr_player")]
public class Destiny2PgcrPlayer : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("instance_id")]
    public long InstanceId { get; set; }

    [Column("character_id")]
    [StringLength(64)]
    [Required]
    public string CharacterId { get; set; } = string.Empty;

    [Column("membership_type")]
    public BungieMembershipTypeEnum? MembershipType { get; set; }

    [Column("membership_id")]
    [StringLength(64)]
    public string? MembershipId { get; set; }

    [Column("completed")]
    public bool? Completed { get; set; }

    [Column("start_seconds")]
    public long? StartSeconds { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
