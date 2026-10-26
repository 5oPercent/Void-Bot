using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_armor_slot_type")]
public sealed class EraArmorSlotType : IEntity<Guid>
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [StringLength(128)] public string Name { get; set; } = string.Empty;
}
