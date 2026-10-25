using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_mod_definition")]
public sealed class EraWeaponModDefinition : IEntity<Guid>
{
    [Key] public Guid Id { get; set; }
    [StringLength(128)] public string Name { get; set; } = string.Empty;
    public Guid SlotTypeId { get; set; }
    [StringLength(2048)] public string Description { get; set; } = string.Empty;
    [StringLength(256)] public string Icon { get; set; } = string.Empty;
}
