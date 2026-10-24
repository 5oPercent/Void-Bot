using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_template_mod_slot")]
public sealed class EraWeaponTemplateModSlot : IEntityBase
{
    public Guid TemplateId { get; set; }
    public EraWeaponModSlot Slot { get; set; }
    public int CandidateCount { get; set; }
    public int DisplayOrder { get; set; }
}
