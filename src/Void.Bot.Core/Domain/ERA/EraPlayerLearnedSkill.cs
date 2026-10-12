using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_player_learned_skill")]
[Index(nameof(PlayerId), nameof(SkillId), IsUnique = true)]
public class EraPlayerLearnedSkill : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }

    [StringLength(128)]
    public string SkillId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}
