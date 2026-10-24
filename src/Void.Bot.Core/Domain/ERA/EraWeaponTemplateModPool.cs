using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_template_mod_pool")]
public sealed class EraWeaponTemplateModPool : IEntityBase
{
    public Guid TemplateId { get; set; }
    public EraWeaponModSlot Slot { get; set; }
    public Guid ModDefinitionId { get; set; }
    public int Weight { get; set; }
}
