using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_player_skill_profile")]
[Index(nameof(PlayerId), IsUnique = true)]
public class EraPlayerSkillProfile : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key]
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public long Version { get; set; }

    [Column(TypeName = "mediumtext")]
    public string Payload { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
