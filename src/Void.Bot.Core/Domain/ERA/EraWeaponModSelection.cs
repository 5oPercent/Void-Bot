using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_mod_selection")]
public sealed class EraWeaponModSelection : IEntityBase
{
    public Guid EquipmentId { get; set; }
    public EraWeaponModSlot Slot { get; set; }
    public Guid SelectedCandidateId { get; set; }
}
