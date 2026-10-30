using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_instance_mod_selection")]
public sealed class EraWeaponInstanceModSelection : IEntityBase
{
    public Guid EquipmentInstanceId { get; set; }
    public Guid SlotId { get; set; }
    [System.ComponentModel.DataAnnotations.StringLength(128)] public string Name { get; set; } = string.Empty;
    public Guid SlotTypeId { get; set; }
    public int DisplayOrder { get; set; }
    public Guid SelectedCandidateId { get; set; }
}
