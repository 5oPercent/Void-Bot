using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Destiny2;

[Table("destiny2_pgcr")]
public class Destiny2Pgcr : IEntity<long>, IEntityCreated, IEntityModified
{
    [Column("instance_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Column("period")]
    public DateTimeOffset Period { get; set; }

    [Column("reference_id")]
    public uint ReferenceId { get; set; }

    [Column("starting_phase_index")]
    public int? StartingPhaseIndex { get; set; }

    [Column("started_from_beginning")]
    public bool? StartedFromBeginning { get; set; }

    [Column("data_version")]
    public int DataVersion { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}