using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_mod_candidate")]
public sealed class EraWeaponModCandidate : IEntityBase
{
    [Key] public Guid CandidateId { get; set; }
    public Guid EquipmentId { get; set; }
    public EraWeaponModSlot Slot { get; set; }
    public Guid ModDefinitionId { get; set; }
    public int DisplayOrder { get; set; }
}
