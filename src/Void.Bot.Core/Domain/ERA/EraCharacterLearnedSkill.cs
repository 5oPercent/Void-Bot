using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_character_learned_skill")]
[Index(nameof(CharacterId), nameof(SkillId), IsUnique = true)]
public class EraCharacterLearnedSkill : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }

    [StringLength(128)]
    public string SkillId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}
