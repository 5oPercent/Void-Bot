using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Core.Domain.Destiny2;

[Table("destiny2_season_history")]
public class Destiny2SeasonHistory : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("bungie_id")]
    [StringLength(64)]
    [Required]
    public string BungieId { get; set; } = string.Empty;
    
    [Column("membership_type")]
    [StringLength(64)]
    [Required]
    public BungieMembershipTypeEnum MembershipType { get; set; } = BungieMembershipTypeEnum.None;
    
    [Column("membership_id")]
    [StringLength(64)]
    [Required]
    public string MembershipId { get; set; } = string.Empty;

    [Column("season_hash")]
    public uint SeasonHash { get; set; }

    [Column("hunter_seconds")]
    public long HunterSeconds { get; set; }

    [Column("titan_seconds")]
    public long TitanSeconds { get; set; }

    [Column("warlock_seconds")]
    public long WarlockSeconds { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}